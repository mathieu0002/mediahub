using System.Text.Json.Serialization;

namespace MediaHub.Infrastructure.Providers.AniList.Models;

public class AniListMedia
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public AniListTitle? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("coverImage")]
    public AniListCoverImage? CoverImage { get; set; }

    [JsonPropertyName("bannerImage")]
    public string? BannerImage { get; set; }

    [JsonPropertyName("startDate")]
    public AniListDate? StartDate { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("chapters")]
    public int? Chapters { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    [JsonPropertyName("averageScore")]
    public int? AverageScore { get; set; }

    [JsonPropertyName("externalLinks")]
    public List<AniListExternalLink> ExternalLinks { get; set; } = new();
}

public class AniListTitle
{
    [JsonPropertyName("romaji")]
    public string? Romaji { get; set; }

    [JsonPropertyName("english")]
    public string? English { get; set; }

    [JsonPropertyName("native")]
    public string? Native { get; set; }
}

public class AniListCoverImage
{
    [JsonPropertyName("large")]
    public string? Large { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }
}

public class AniListDate
{
    [JsonPropertyName("year")]
    public int? Year { get; set; }
}

public class AniListExternalLink
{
    [JsonPropertyName("site")]
    public string Site { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}