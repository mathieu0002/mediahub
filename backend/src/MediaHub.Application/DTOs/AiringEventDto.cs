namespace MediaHub.Application.DTOs;

public record AiringEventDto
{
    public required int MediaItemId { get; init; }
    public required string Title { get; init; }
    public string? CoverImageUrl { get; init; }
    public required int EpisodeNumber { get; init; }
    public required DateTime AiringAt { get; init; }  // UTC
}