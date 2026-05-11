using MediaHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaHub.Infrastructure.Persistence.Configurations;

public class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.ToTable("media_items");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Source).HasConversion<string>().HasMaxLength(20);

        builder.Property(m => m.ExternalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.OriginalTitle).HasMaxLength(500);
        builder.Property(m => m.Synopsis).HasColumnType("text");
        builder.Property(m => m.CoverImageUrl).HasMaxLength(1000);
        builder.Property(m => m.BannerImageUrl).HasMaxLength(1000);

        builder.Property(m => m.Genres)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(m => m.AvailableOn)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // Index unique sur (Source, ExternalId, Type) pour éviter les doublons
        builder.HasIndex(m => new { m.Source, m.ExternalId, m.Type }).IsUnique();
    }
}