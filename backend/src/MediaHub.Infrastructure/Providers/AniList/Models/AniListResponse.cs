using System.Text.Json.Serialization;

namespace MediaHub.Infrastructure.Providers.AniList.Models;

public class AniListResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<AniListError>? Errors { get; set; }
}

public class AniListError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class AniListSearchData
{
    [JsonPropertyName("Page")]
    public AniListPage? Page { get; set; }
}

public class AniListSinglePageData
{
    [JsonPropertyName("Media")]
    public AniListMedia? Media { get; set; }
}

public class AniListPage
{
    [JsonPropertyName("pageInfo")]
    public AniListPageInfo? PageInfo { get; set; }

    [JsonPropertyName("media")]
    public List<AniListMedia> Media { get; set; } = new();
}

public class AniListPageInfo
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }
}