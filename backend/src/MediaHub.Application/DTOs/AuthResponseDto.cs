namespace MediaHub.Application.DTOs;

public record AuthResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime AccessTokenExpiresAt { get; init; }
    public required UserDto User { get; init; }
}

public record UserDto
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }
}