using System.Text.Json.Serialization;

namespace MediaHub.Infrastructure.Providers.Tmdb.Models;

public class TmdbSearchResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("total_results")] public int TotalResults { get; set; }
    [JsonPropertyName("results")] public List<TmdbSearchItem> Results { get; set; } = new();
}

public class TmdbSearchItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("original_title")] public string? OriginalTitle { get; set; }
    [JsonPropertyName("original_name")] public string? OriginalName { get; set; }
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; set; } = new();
    [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
}