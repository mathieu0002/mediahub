using System.Text.Json.Serialization;

namespace MediaHub.Infrastructure.Providers.Tmdb.Models;

public class TmdbMovieDetail
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("original_title")] public string? OriginalTitle { get; set; }
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; } = new();
    [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
    [JsonPropertyName("watch/providers")] public TmdbWatchProvidersWrapper? WatchProviders { get; set; }
}

public class TmdbTvDetail
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("original_name")] public string? OriginalName { get; set; }
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; } = new();
    [JsonPropertyName("number_of_episodes")] public int? NumberOfEpisodes { get; set; }
    [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
    [JsonPropertyName("watch/providers")] public TmdbWatchProvidersWrapper? WatchProviders { get; set; }
    [JsonPropertyName("seasons")] public List<TmdbTvSeason> Seasons { get; set; } = new();
    [JsonPropertyName("status")] public string? Status { get; set; }
}

public class TmdbGenre
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class TmdbWatchProvidersWrapper
{
    [JsonPropertyName("results")] public Dictionary<string, TmdbCountryProviders> Results { get; set; } = new();
}

public class TmdbCountryProviders
{
    [JsonPropertyName("flatrate")] public List<TmdbProviderInfo> Flatrate { get; set; } = new();
}

public class TmdbProviderInfo
{
    [JsonPropertyName("provider_name")] public string ProviderName { get; set; } = string.Empty;
}

public class TmdbTvEpisode
{
    [JsonPropertyName("episode_number")] public int EpisodeNumber { get; set; }
    [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
    [JsonPropertyName("air_date")] public string? AirDate { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

public class TmdbTvSeason
{
    [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
    [JsonPropertyName("episode_count")] public int EpisodeCount { get; set; }
    [JsonPropertyName("air_date")] public string? AirDate { get; set; }
}

public class TmdbSeasonDetail
{
    [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
    [JsonPropertyName("episodes")] public List<TmdbTvEpisode> Episodes { get; set; } = new();
}