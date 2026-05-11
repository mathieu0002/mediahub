using MediaHub.Domain.Enums;

namespace MediaHub.Application.DTOs.Requests;

public record UpdateUserMediaRequest
{
    public WatchStatus? Status { get; init; }
    public int? Progress { get; init; }
    public int? UserScore { get; init; }
    public string? Notes { get; init; }
}