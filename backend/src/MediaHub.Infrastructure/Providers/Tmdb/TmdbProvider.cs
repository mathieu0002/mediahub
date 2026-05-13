using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Domain.Enums;
using MediaHub.Infrastructure.Providers.Tmdb.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using DomainMediaType = MediaHub.Domain.Enums.MediaType;

namespace MediaHub.Infrastructure.Providers.Tmdb;

public class TmdbProvider : IMediaProvider, IUpcomingProvider
{
    private readonly HttpClient _http;
    private readonly TmdbOptions _options;
    private readonly ILogger<TmdbProvider> _logger;

    public MediaSource Source => MediaSource.Tmdb;

    public TmdbProvider(HttpClient http, IOptions<TmdbOptions> options, ILogger<TmdbProvider> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public bool SupportsType(DomainMediaType type)
        => type is DomainMediaType.Movie or DomainMediaType.TvShow;

    public async Task<Result<MediaSearchResultDto>> SearchAsync(
        string query, DomainMediaType type, int page, int pageSize, CancellationToken ct = default)
    {
        var endpoint = type == DomainMediaType.Movie ? "search/movie" : "search/tv";
        var url = $"{endpoint}?query={Uri.EscapeDataString(query)}&page={page}&language={_options.Language}&region={_options.Region}";

        try
        {
            var response = await _http.GetFromJsonAsync<TmdbSearchResponse>(url, ct);
            if (response is null)
                return Result.Failure<MediaSearchResultDto>("Réponse TMDB vide");

            var items = response.Results
                .Take(pageSize)
                .Select(r => MapSearchToDto(r, type))
                .ToList();

            return Result.Success(new MediaSearchResultDto
            {
                Items = items,
                TotalResults = response.TotalResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TMDB search failed");
            return Result.Failure<MediaSearchResultDto>($"TMDB error: {ex.Message}");
        }
    }

    public async Task<Result<MediaItemDto>> GetByIdAsync(
        string externalId, DomainMediaType type, CancellationToken ct = default)
    {
        try
        {
            var endpoint = type == DomainMediaType.Movie ? "movie" : "tv";
            var url = $"{endpoint}/{externalId}?language={_options.Language}&append_to_response=watch/providers";

            if (type == DomainMediaType.Movie)
            {
                var movie = await _http.GetFromJsonAsync<TmdbMovieDetail>(url, ct);
                return movie is null
                    ? Result.Failure<MediaItemDto>("Film introuvable")
                    : Result.Success(MapMovieToDto(movie));
            }
            else
            {
                var tv = await _http.GetFromJsonAsync<TmdbTvDetail>(url, ct);
                return tv is null
                    ? Result.Failure<MediaItemDto>("Série introuvable")
                    : Result.Success(MapTvToDto(tv));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TMDB detail failed");
            return Result.Failure<MediaItemDto>($"TMDB error: {ex.Message}");
        }
    }

    private MediaItemDto MapSearchToDto(TmdbSearchItem r, DomainMediaType type)
    {
        var isMovie = type == DomainMediaType.Movie;
        return new MediaItemDto
        {
            ExternalId = r.Id.ToString(),
            Source = MediaSource.Tmdb,
            Type = type,
            Title = (isMovie ? r.Title : r.Name) ?? "Sans titre",
            OriginalTitle = isMovie ? r.OriginalTitle : r.OriginalName,
            Synopsis = r.Overview,
            CoverImageUrl = string.IsNullOrEmpty(r.PosterPath) ? null : _options.ImageBaseUrl + r.PosterPath,
            BannerImageUrl = string.IsNullOrEmpty(r.BackdropPath) ? null : _options.BannerBaseUrl + r.BackdropPath,
            ReleaseYear = ParseYear(isMovie ? r.ReleaseDate : r.FirstAirDate),
            Genres = [],
            AvailableOn = [],
            ExternalScore = r.VoteAverage
        };
    }

    private MediaItemDto MapMovieToDto(TmdbMovieDetail m) => new()
    {
        ExternalId = m.Id.ToString(),
        Source = MediaSource.Tmdb,
        Type = DomainMediaType.Movie,
        Title = m.Title ?? "Sans titre",
        OriginalTitle = m.OriginalTitle,
        Synopsis = m.Overview,
        CoverImageUrl = string.IsNullOrEmpty(m.PosterPath) ? null : _options.ImageBaseUrl + m.PosterPath,
        BannerImageUrl = string.IsNullOrEmpty(m.BackdropPath) ? null : _options.BannerBaseUrl + m.BackdropPath,
        ReleaseYear = ParseYear(m.ReleaseDate),
        Genres = m.Genres.Select(g => g.Name).ToList(),
        AvailableOn = ExtractProviders(m.WatchProviders),
        ExternalScore = m.VoteAverage
    };

    private MediaItemDto MapTvToDto(TmdbTvDetail t) => new()
    {
        ExternalId = t.Id.ToString(),
        Source = MediaSource.Tmdb,
        Type = DomainMediaType.TvShow,
        Title = t.Name ?? "Sans titre",
        OriginalTitle = t.OriginalName,
        Synopsis = t.Overview,
        CoverImageUrl = string.IsNullOrEmpty(t.PosterPath) ? null : _options.ImageBaseUrl + t.PosterPath,
        BannerImageUrl = string.IsNullOrEmpty(t.BackdropPath) ? null : _options.BannerBaseUrl + t.BackdropPath,
        ReleaseYear = ParseYear(t.FirstAirDate),
        TotalUnits = t.NumberOfEpisodes,
        Genres = t.Genres.Select(g => g.Name).ToList(),
        AvailableOn = ExtractProviders(t.WatchProviders),
        ExternalScore = t.VoteAverage
    };

    public async Task<Result<IReadOnlyList<AiringEventDto>>> GetUpcomingAsync(
    string externalId,
    DomainMediaType type,
    DateTime fromUtc,
    DateTime toUtc,
    CancellationToken ct = default)
{
    // TMDB Movies : pas vraiment de "next episode", on pourrait vérifier la release_date
    // mais c'est rare qu'un film soit dans ta bibliothèque "Watching" avant sa sortie.
    // → On gère uniquement les séries pour l'instant.
    if (type != DomainMediaType.TvShow)
        return Result.Success<IReadOnlyList<AiringEventDto>>([]);

    try
    {
        var url = $"tv/{externalId}?language={_options.Language}";
        var tv = await _http.GetFromJsonAsync<TmdbTvDetail>(url, ct);
        if (tv is null)
            return Result.Success<IReadOnlyList<AiringEventDto>>([]);

        // Si la série est terminée, pas la peine de chercher
        if (tv.Status == "Ended" || tv.Status == "Canceled")
            return Result.Success<IReadOnlyList<AiringEventDto>>([]);

        var events = new List<AiringEventDto>();

        // On regarde la saison la plus récente (qui n'est pas la saison 0 = specials)
        var currentSeason = tv.Seasons
            .Where(s => s.SeasonNumber > 0)
            .OrderByDescending(s => s.SeasonNumber)
            .FirstOrDefault();

        if (currentSeason is null)
            return Result.Success<IReadOnlyList<AiringEventDto>>([]);

        var seasonUrl = $"tv/{externalId}/season/{currentSeason.SeasonNumber}?language={_options.Language}";
        var season = await _http.GetFromJsonAsync<TmdbSeasonDetail>(seasonUrl, ct);

        if (season is null)
            return Result.Success<IReadOnlyList<AiringEventDto>>(events);

        var title = tv.Name ?? "Sans titre";
        var coverUrl = string.IsNullOrEmpty(tv.PosterPath)
            ? null
            : _options.ImageBaseUrl + tv.PosterPath;

        foreach (var ep in season.Episodes)
        {
            if (!DateTime.TryParse(ep.AirDate, out var airDate))
                continue;

            var airDateUtc = DateTime.SpecifyKind(airDate, DateTimeKind.Utc);
            if (airDateUtc < fromUtc || airDateUtc > toUtc)
                continue;

            if (int.TryParse(externalId, out var mediaIdInt))
            {
                events.Add(new AiringEventDto
                {
                    MediaItemId = mediaIdInt,
                    Title = $"{title} S{ep.SeasonNumber:D2}",
                    CoverImageUrl = coverUrl,
                    EpisodeNumber = ep.EpisodeNumber,
                    AiringAt = airDateUtc
                });
            }
        }

        IReadOnlyList<AiringEventDto> ordered = events.OrderBy(e => e.AiringAt).ToList();
        return Result.Success(ordered);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "TMDB upcoming failed for {ExternalId}", externalId);
        return Result.Failure<IReadOnlyList<AiringEventDto>>($"TMDB error: {ex.Message}");
    }
}
    
    private List<string> ExtractProviders(TmdbWatchProvidersWrapper? wrapper)
    {
        if (wrapper is null || !wrapper.Results.TryGetValue(_options.Region, out var country))
            return [];

        return country.Flatrate.Select(p => p.ProviderName).Distinct().ToList();
    }

    private static int? ParseYear(string? date)
        => DateTime.TryParse(date, out var d) ? d.Year : null;
}