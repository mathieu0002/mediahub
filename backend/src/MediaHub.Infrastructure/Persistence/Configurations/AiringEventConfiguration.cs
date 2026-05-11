using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaHub.Infrastructure.Persistence.Configurations;

public class AiringEventConfiguration : IEntityTypeConfiguration<AiringEvent>
{
    public void Configure(EntityTypeBuilder<AiringEvent> builder)
    {
        builder.ToTable("airing_events");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.MediaItem)
            .WithMany()
            .HasForeignKey(a => a.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.AiringAt);
        builder.HasIndex(a => new { a.MediaItemId, a.EpisodeNumber }).IsUnique();
    }
}