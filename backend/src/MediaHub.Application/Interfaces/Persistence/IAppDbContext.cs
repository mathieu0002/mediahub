using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Application.Interfaces.Persistence;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<MediaItem> MediaItems { get; }
    DbSet<UserMediaItem> UserMediaItems { get; }
    DbSet<AiringEvent> AiringEvents { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}