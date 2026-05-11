using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Domain.Enums;
using MediaHub.Infrastructure.Providers.AniList.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using DomainMediaType = MediaHub.Domain.Enums.MediaType;

namespace MediaHub.Infrastructure.Providers.AniList;

public class AniListProvider : IMediaProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<AniListProvider> _logger;

    public MediaSource Source => MediaSource.AniList;

    public AniListProvider(HttpClient http, ILogger<AniListProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool SupportsType(DomainMediaType type)
        => type is DomainMediaType.Anime or DomainMediaType.Manga;

    public async Task<Result<MediaSearchResultDto>> SearchAsync(
        string query, DomainMediaType type, int page, int pageSize, CancellationToken ct = default)
    {
        var variables = new
        {
            search = query,
            type = type == DomainMediaType.Anime ? "ANIME" : "MANGA",
            page,
            perPage = pageSize
        };

        var response = await PostGraphQLAsync<AniListSearchData>(
            AniListGraphQLQueries.Search, variables, ct);

        if (response.IsFailure)
            return Result.Failure<MediaSearchResultDto>(response.Error!);

        var pageData = response.Value?.Page;
        if (pageData is null)
            return Result.Failure<MediaSearchResultDto>("Réponse AniList vide");

        var items = pageData.Media
            .Select(m => MapToDto(m, type))
            .ToList();

        return Result.Success(new MediaSearchResultDto
        {
            Items = items,
            TotalResults = pageData.PageInfo?.Total ?? items.Count
        });
    }

    public async Task<Result<MediaItemDto>> GetByIdAsync(
        string externalId, DomainMediaType type, CancellationToken ct = default)
    {
        if (!int.TryParse(externalId, out var id))
            return Result.Failure<MediaItemDto>("ID AniList invalide");

        var response = await PostGraphQLAsync<AniListSinglePageData>(
            AniListGraphQLQueries.GetById, new { id }, ct);

        if (response.IsFailure)
            return Result.Failure<MediaItemDto>(response.Error!);

        var media = response.Value?.Media;
        if (media is null)
            return Result.Failure<MediaItemDto>("Œuvre introuvable sur AniList");

        return Result.Success(MapToDto(media, type));
    }

    private async Task<Result<T>> PostGraphQLAsync<T>(
        string query, object variables, CancellationToken ct) where T : class
    {
        try
        {
            var payload = new { query, variables };
            var response = await _http.PostAsJsonAsync("", payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AniList request failed: {Status}", response.StatusCode);
                return Result.Failure<T>($"AniList: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var parsed = JsonSerializer.Deserialize<AniListResponse<T>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.Errors is { Count: > 0 })
            {
                var err = string.Join("; ", parsed.Errors.Select(e => e.Message));
                _logger.LogWarning("AniList GraphQL errors: {Errors}", err);
                return Result.Failure<T>(err);
            }

            return parsed?.Data is null
                ? Result.Failure<T>("Réponse AniList vide")
                : Result.Success(parsed.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AniList call failed");
            return Result.Failure<T>($"AniList error: {ex.Message}");
        }
    }

    private MediaItemDto MapToDto(AniListMedia media, DomainMediaType type)
    {
        var title = media.Title?.English
            ?? media.Title?.Romaji
            ?? media.Title?.Native
            ?? "Sans titre";

        var availableOn = media.ExternalLinks
            .Where(l => l.Type.Equals("STREAMING", StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Site)
            .Distinct()
            .ToList();

        return new MediaItemDto
        {
            ExternalId = media.Id.ToString(),
            Source = MediaSource.AniList,
            Type = type,
            Title = title,
            OriginalTitle = media.Title?.Native,
            Synopsis = CleanHtml(media.Description),
            CoverImageUrl = media.CoverImage?.Large,
            BannerImageUrl = media.BannerImage,
            ReleaseYear = media.StartDate?.Year,
            TotalUnits = type == DomainMediaType.Anime ? media.Episodes : media.Chapters,
            Genres = media.Genres,
            AvailableOn = availableOn,
            ExternalScore = media.AverageScore.HasValue ? media.AverageScore.Value / 10.0 : null
        };
    }

    private static string? CleanHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
    }
}