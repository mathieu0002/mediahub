using MediaHub.Domain.Enums;

namespace MediaHub.Application.DTOs.Requests;

public record CreateUserMediaRequest
{
    public required string ExternalId { get; init; }
    public required MediaSource Source { get; init; }
    public required MediaType Type { get; init; }
    public WatchStatus Status { get; init; } = WatchStatus.Planning;
}