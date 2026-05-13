using MediaHub.Domain.Entities;

namespace MediaHub.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}