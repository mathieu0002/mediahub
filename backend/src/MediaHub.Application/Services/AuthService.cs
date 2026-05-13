using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAppDbContext db,
        IJwtTokenGenerator tokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u =>
            u.Email == emailNormalized || u.Username == request.Username, ct);

        if (exists)
            return Result.Failure<AuthResponseDto>("Email ou nom d'utilisateur déjà pris");

        var user = new User
        {
            Email = emailNormalized,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentative de login échouée pour {Email}", emailNormalized);
            return Result.Failure<AuthResponseDto>("Identifiants invalides");
        }

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var refreshToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (refreshToken is null || !refreshToken.IsActive)
            return Result.Failure<AuthResponseDto>("Refresh token invalide ou expiré");

        // Révoquer l'ancien (rotation)
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(refreshToken.User, ct);
    }

    public async Task<Result> RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, ct);
        if (rt is null) return Result.Failure("Token introuvable");

        rt.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<AuthResponseDto>> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _tokenGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            }
        });
    }
}