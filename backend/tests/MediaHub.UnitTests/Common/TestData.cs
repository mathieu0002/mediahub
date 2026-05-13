using MediaHub.Domain.Entities;
using MediaHub.Domain.Enums;

namespace MediaHub.UnitTests.Common;

public static class TestData
{
    public static User CreateUser(int id = 1, string email = "test@test.com", string username = "testuser")
        => new()
        {
            Id = id,
            Email = email,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"),
            CreatedAt = DateTime.UtcNow
        };

    public static MediaItem CreateMediaItem(
        int id = 1,
        string externalId = "20",
        MediaType type = MediaType.Anime,
        MediaSource source = MediaSource.AniList,
        string title = "Naruto")
        => new()
        {
            Id = id,
            ExternalId = externalId,
            Source = source,
            Type = type,
            Title = title,
            Genres = ["Action", "Adventure"],
            AvailableOn = ["Crunchyroll"],
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };

    public static UserMediaItem CreateUserMediaItem(
        int id = 1,
        int userId = 1,
        int mediaItemId = 1,
        WatchStatus status = WatchStatus.Watching)
        => new()
        {
            Id = id,
            UserId = userId,
            MediaItemId = mediaItemId,
            Status = status,
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };
}