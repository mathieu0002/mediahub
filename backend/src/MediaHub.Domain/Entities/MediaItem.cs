using MediaHub.Domain.Common;
using MediaHub.Domain.Enums;

namespace MediaHub.Domain.Entities;

/// <summary>
/// Métadonnées d'une œuvre récupérées depuis une source externe (AniList, TMDB...).
/// Une même œuvre peut être référencée par plusieurs UserMediaItem.
/// </summary>
public class MediaItem : BaseEntity
{
    public MediaType Type { get; set; }
    public MediaSource Source { get; set; }
    public string ExternalId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Synopsis { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? BannerImageUrl { get; set; }

    public int? ReleaseYear { get; set; }
    public int? TotalUnits { get; set; }  // épisodes pour anime/série, chapitres pour manga, null pour film

    public List<string> Genres { get; set; } = new();
    public List<string> AvailableOn { get; set; } = new();  // ["Netflix", "Crunchyroll"]

    public double? ExternalScore { get; set; }  // note moyenne sur la source externe
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
}