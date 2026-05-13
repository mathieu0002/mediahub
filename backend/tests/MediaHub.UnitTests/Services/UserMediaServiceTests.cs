using FluentAssertions;
using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Application.Services;
using MediaHub.Domain.Enums;
using MediaHub.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MediaHub.UnitTests.Services;

public class UserMediaServiceTests
{
    private readonly Mock<IMediaSearchService> _searchMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    private UserMediaService CreateSut(MediaHub.Infrastructure.Persistence.AppDbContext db)
        => new(db, _searchMock.Object, _cacheMock.Object, NullLogger<UserMediaService>.Instance);

    [Fact]
    public async Task AddAsync_WithNewMediaItem_FetchesFromProviderAndAddsToLibrary()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        await db.SaveChangesAsync();

        var mediaDto = new MediaItemDto
        {
            ExternalId = "20",
            Source = MediaSource.AniList,
            Type = MediaType.Anime,
            Title = "Naruto",
            Genres = ["Action"],
            AvailableOn = []
        };

        _searchMock
            .Setup(s => s.GetDetailsAsync("20", MediaType.Anime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mediaDto));

        var sut = CreateSut(db);

        // Act
        var result = await sut.AddAsync(1, new CreateUserMediaRequest
        {
            ExternalId = "20",
            Source = MediaSource.AniList,
            Type = MediaType.Anime,
            Status = WatchStatus.Watching
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(WatchStatus.Watching);
        result.Value.Media.Title.Should().Be("Naruto");

        db.MediaItems.Should().HaveCount(1);
        db.UserMediaItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_WithExistingMediaItem_ReusesExistingAndDoesNotCallProvider()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        db.MediaItems.Add(TestData.CreateMediaItem(id: 1, externalId: "20"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.AddAsync(1, new CreateUserMediaRequest
        {
            ExternalId = "20",
            Source = MediaSource.AniList,
            Type = MediaType.Anime,
            Status = WatchStatus.Planning
        });

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Le provider ne doit pas avoir été appelé puisque le MediaItem existe déjà
        _searchMock.Verify(
            s => s.GetDetailsAsync(It.IsAny<string>(), It.IsAny<MediaType>(), It.IsAny<CancellationToken>()),
            Times.Never);

        db.MediaItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_WhenAlreadyInLibrary_ReturnsFailure()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        db.MediaItems.Add(TestData.CreateMediaItem());
        db.UserMediaItems.Add(TestData.CreateUserMediaItem());
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.AddAsync(1, new CreateUserMediaRequest
        {
            ExternalId = "20",
            Source = MediaSource.AniList,
            Type = MediaType.Anime
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Déjà");
    }

    [Fact]
    public async Task UpdateAsync_ToCompleted_SetsCompletedAt()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        db.MediaItems.Add(TestData.CreateMediaItem());
        db.UserMediaItems.Add(TestData.CreateUserMediaItem(status: WatchStatus.Watching));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.UpdateAsync(1, 1, new UpdateUserMediaRequest
        {
            Status = WatchStatus.Completed
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(WatchStatus.Completed);
        result.Value.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_DoesNotOverwriteOtherFields()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        db.MediaItems.Add(TestData.CreateMediaItem());
        var entity = TestData.CreateUserMediaItem(status: WatchStatus.Watching);
        entity.UserScore = 80;
        entity.Notes = "Initial note";
        db.UserMediaItems.Add(entity);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act : on update QUE la progression
        var result = await sut.UpdateAsync(1, 1, new UpdateUserMediaRequest
        {
            Progress = 50
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Progress.Should().Be(50);
        result.Value.UserScore.Should().Be(80);        // pas écrasé
        result.Value.Notes.Should().Be("Initial note"); // pas écrasé
    }

    [Fact]
    public async Task UpdateAsync_WithDifferentUserId_ReturnsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser(id: 1));
        db.Users.Add(TestData.CreateUser(id: 2, email: "other@test.com", username: "other"));
        db.MediaItems.Add(TestData.CreateMediaItem());
        db.UserMediaItems.Add(TestData.CreateUserMediaItem(userId: 1));  // appartient à user 1
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act : user 2 essaie d'update une entrée de user 1
        var result = await sut.UpdateAsync(userId: 2, userMediaId: 1, new UpdateUserMediaRequest
        {
            Progress = 100
        });

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_ExistingItem_DeletesIt()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());
        db.MediaItems.Add(TestData.CreateMediaItem());
        db.UserMediaItems.Add(TestData.CreateUserMediaItem());
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        // Act
        var result = await sut.RemoveAsync(1, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        db.UserMediaItems.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MediaType.Anime, null, 2)]
    [InlineData(MediaType.Manga, null, 1)]
    [InlineData(null, WatchStatus.Completed, 1)]
    public async Task GetLibraryAsync_AppliesFiltersCorrectly(
        MediaType? typeFilter, WatchStatus? statusFilter, int expectedCount)
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(TestData.CreateUser());

        db.MediaItems.AddRange(
            TestData.CreateMediaItem(id: 1, type: MediaType.Anime, title: "Naruto"),
            TestData.CreateMediaItem(id: 2, type: MediaType.Anime, title: "One Piece"),
            TestData.CreateMediaItem(id: 3, type: MediaType.Manga, title: "Berserk")
        );

        db.UserMediaItems.AddRange(
            TestData.CreateUserMediaItem(id: 1, mediaItemId: 1, status: WatchStatus.Watching),
            TestData.CreateUserMediaItem(id: 2, mediaItemId: 2, status: WatchStatus.Completed),
            TestData.CreateUserMediaItem(id: 3, mediaItemId: 3, status: WatchStatus.Watching)
        );

        await db.SaveChangesAsync();
        var sut = CreateSut(db);

        // Act
        var result = await sut.GetLibraryAsync(1, typeFilter, statusFilter);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(expectedCount);
    }
}