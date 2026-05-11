using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MediaHub.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<UserMediaItem> UserMediaItems => Set<UserMediaItem>();
    public DbSet<AiringEvent> AiringEvents => Set<AiringEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}