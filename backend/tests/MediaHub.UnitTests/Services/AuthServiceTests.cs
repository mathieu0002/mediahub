using FluentAssertions;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Application.Services;
using MediaHub.Domain.Entities;
using MediaHub.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace MediaHub.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenGenerator> _tokenGenMock;
    private readonly JwtOptions _jwtOptions;

    public AuthServiceTests()
    {
        _tokenGenMock = new Mock<IJwtTokenGenerator>();
        _tokenGenMock.Setup(g => g.GenerateAccessToken(It.IsAny<User>())).Returns("fake-access-token");
        _tokenGenMock.Setup(g => g.GenerateRefreshToken()).Returns("fake-refresh-token");

        _jwtOptions = new JwtOptions
        {
            Issuer = "Test",
            Audience = "Test",
            SigningKey = "test-key-at-least-32-chars-long-aaaaaaaaaaaaaaaaaaaaaaaaaa",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };
    }

    private AuthService CreateSut(MediaHub.Infrastructure.Persistence.AppDbContext db)
    {
        return new AuthService(
            db,
            _tokenGenMock.Object,
            Options.Create(_jwtOptions),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessAndCreatesUser()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Username = "newuser",
            Password = "Test1234"
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("fake-access-token");
        result.Value.RefreshToken.Should().Be("fake-refresh-token");
        result.Value.User.Email.Should().Be("new@test.com");

        db.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser(email: "existing@test.com"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Username = "otheruser",
            Password = "Test1234"
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("déjà pris");
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);
        var request = new RegisterRequest
        {
            Email = "  MIXED@CASE.COM  ",
            Username = "user",
            Password = "Test1234"
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var savedUser = db.Users.Single();
        savedUser.Email.Should().Be("mixed@case.com");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser(email: "user@test.com"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "Test1234"
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser(email: "user@test.com"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "WrongPassword"
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Identifiants invalides");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "ghost@test.com",
            Password = "Test1234"
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Identifiants invalides");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokensAndRevokesOld()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var user = TestData.CreateUser();
        db.Users.Add(user);
        var oldToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "old-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "old-token"
        });

        // Assert
        result.IsSuccess.Should().BeTrue();

        var refreshedOld = await db.RefreshTokens.FindAsync(oldToken.Id);
        refreshedOld!.RevokedAt.Should().NotBeNull();
        db.RefreshTokens.Should().HaveCount(2);  // l'ancien révoqué + le nouveau
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var user = TestData.CreateUser();
        db.Users.Add(user);
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "revoked",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "revoked"
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("invalide");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var user = TestData.CreateUser();
        db.Users.Add(user);
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "expired",
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "expired"
        });

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}