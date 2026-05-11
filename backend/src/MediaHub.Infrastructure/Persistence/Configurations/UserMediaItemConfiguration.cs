using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaHub.Infrastructure.Persistence.Configurations;

public class UserMediaItemConfiguration : IEntityTypeConfiguration<UserMediaItem>
{
    public void Configure(EntityTypeBuilder<UserMediaItem> builder)
    {
        builder.ToTable("user_media_items");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.Notes).HasColumnType("text");

        builder.HasOne(u => u.MediaItem)
            .WithMany()
            .HasForeignKey(u => u.MediaItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un user ne peut avoir un même MediaItem qu'une seule fois
        builder.HasIndex(u => new { u.UserId, u.MediaItemId }).IsUnique();

        builder.HasIndex(u => u.Status);
    }
}