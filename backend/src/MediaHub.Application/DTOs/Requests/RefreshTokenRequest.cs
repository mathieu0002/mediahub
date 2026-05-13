namespace MediaHub.Application.DTOs.Requests;

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}