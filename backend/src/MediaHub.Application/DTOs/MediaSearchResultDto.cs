namespace MediaHub.Application.DTOs;

public record MediaSearchResultDto
{
    public required IReadOnlyList<MediaItemDto> Items { get; init; }
    public required int TotalResults { get; init; }
}