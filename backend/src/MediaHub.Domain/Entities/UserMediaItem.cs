using MediaHub.Domain.Common;
using MediaHub.Domain.Enums;

namespace MediaHub.Domain.Entities;

/// <summary>
/// Représente une œuvre dans la bibliothèque personnelle d'un user.
/// Contient le statut, la progression, la note perso, etc.
/// </summary>
public class UserMediaItem : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MediaItemId { get; set; }
    public MediaItem MediaItem { get; set; } = null!;

    public WatchStatus Status { get; set; } = WatchStatus.Planning;
    public int Progress { get; set; }  // épisodes vus / chapitres lus
    public int? UserScore { get; set; }  // 0-100
    public string? Notes { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}