using System.Text.Json.Serialization;

namespace MediaHub.Infrastructure.Providers.AniList.Models;

public class AniListAiringData
{
    [JsonPropertyName("Media")]
    public AniListMediaWithSchedule? Media { get; set; }
}

public class AniListMediaWithSchedule
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public AniListTitle? Title { get; set; }

    [JsonPropertyName("coverImage")]
    public AniListCoverImage? CoverImage { get; set; }

    [JsonPropertyName("airingSchedule")]
    public AniListAiringScheduleConnection? AiringSchedule { get; set; }
}

public class AniListAiringScheduleConnection
{
    [JsonPropertyName("nodes")]
    public List<AniListAiringNode> Nodes { get; set; } = new();
}

public class AniListAiringNode
{
    [JsonPropertyName("episode")]
    public int Episode { get; set; }

    [JsonPropertyName("airingAt")]
    public long AiringAt { get; set; }  // Unix timestamp en secondes

    [JsonPropertyName("mediaId")]
    public int MediaId { get; set; }
}