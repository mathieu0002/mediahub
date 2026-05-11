using MediaHub.Domain.Enums;

namespace MediaHub.Application.DTOs.Requests;

public record SearchMediaRequest
{
    public required string Query { get; init; }
    public required MediaType Type { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}