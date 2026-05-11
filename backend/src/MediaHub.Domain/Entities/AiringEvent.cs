using MediaHub.Domain.Common;
using MediaHub.Domain.Enums;

namespace MediaHub.Domain.Entities;

public class AiringEvent : BaseEntity
{
    public int MediaItemId { get; set; }
    public MediaItem MediaItem { get; set; } = null!;

    public int EpisodeNumber { get; set; }
    public DateTime AiringAt { get; set; }  // UTC
}