using MediaHub.Domain.Enums;

namespace MediaHub.Application.DTOs;

public record MediaItemDto
{
    public required string ExternalId { get; init; }
    public required MediaSource Source { get; init; }
    public required MediaType Type { get; init; }
    public required string Title { get; init; }
    public string? OriginalTitle { get; init; }
    public string? Synopsis { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? BannerImageUrl { get; init; }
    public int? ReleaseYear { get; init; }
    public int? TotalUnits { get; init; }
    public IReadOnlyList<string> Genres { get; init; } = [];
    public IReadOnlyList<string> AvailableOn { get; init; } = [];
    public double? ExternalScore { get; init; }
}