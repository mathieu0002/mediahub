using MediaHub.Domain.Enums;

namespace MediaHub.Application.DTOs;

public record UserMediaItemDto
{
    public required int Id { get; init; }
    public required MediaItemDto Media { get; init; }
    public required WatchStatus Status { get; init; }
    public required int Progress { get; init; }
    public int? UserScore { get; init; }
    public string? Notes { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}