using FluentAssertions;
using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Application.Services;
using MediaHub.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MediaHub.UnitTests.Services;

public class MediaSearchServiceTests
{
    private readonly Mock<IMediaProvider> _providerMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    private MediaSearchService CreateSut()
    {
        _providerMock.Setup(p => p.SupportsType(It.IsAny<MediaType>())).Returns(true);
        return new MediaSearchService(
            new[] { _providerMock.Object },
            _cacheMock.Object,
            NullLogger<MediaSearchService>.Instance);
    }

    [Fact]
    public async Task SearchAsync_WhenCacheHit_DoesNotCallProvider()
    {
        // Arrange
        var cached = new MediaSearchResultDto { Items = [], TotalResults = 0 };
        _cacheMock
            .Setup(c => c.GetAsync<MediaSearchResultDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(new SearchMediaRequest
        {
            Query = "naruto",
            Type = MediaType.Anime
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        _providerMock.Verify(
            p => p.SearchAsync(It.IsAny<string>(), It.IsAny<MediaType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenCacheMiss_CallsProviderAndCachesResult()
    {
        // Arrange
        var providerResult = new MediaSearchResultDto { Items = [], TotalResults = 0 };

        _cacheMock
            .Setup(c => c.GetAsync<MediaSearchResultDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaSearchResultDto?)null);

        _providerMock
            .Setup(p => p.SearchAsync("naruto", MediaType.Anime, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(providerResult));

        var sut = CreateSut();

        // Act
        var result = await sut.SearchAsync(new SearchMediaRequest
        {
            Query = "naruto",
            Type = MediaType.Anime
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        _providerMock.Verify(
            p => p.SearchAsync("naruto", MediaType.Anime, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<MediaSearchResultDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenNoProviderSupportsType_ReturnsFailure()
    {
        // Arrange
        _providerMock.Setup(p => p.SupportsType(It.IsAny<MediaType>())).Returns(false);

        var sut = new MediaSearchService(
            new[] { _providerMock.Object },
            _cacheMock.Object,
            NullLogger<MediaSearchService>.Instance);

        // Act
        var result = await sut.SearchAsync(new SearchMediaRequest
        {
            Query = "test",
            Type = MediaType.Anime
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Aucun provider");
    }
}