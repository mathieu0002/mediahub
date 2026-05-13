using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<Result> RevokeAsync(string refreshToken, CancellationToken ct = default);
}