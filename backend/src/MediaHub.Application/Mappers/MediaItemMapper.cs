using MediaHub.Application.DTOs;
using MediaHub.Domain.Entities;

namespace MediaHub.Application.Mappers;

public static class MediaItemMapper
{
    public static MediaItemDto ToDto(this MediaItem entity) => new()
    {
        ExternalId = entity.ExternalId,
        Source = entity.Source,
        Type = entity.Type,
        Title = entity.Title,
        OriginalTitle = entity.OriginalTitle,
        Synopsis = entity.Synopsis,
        CoverImageUrl = entity.CoverImageUrl,
        BannerImageUrl = entity.BannerImageUrl,
        ReleaseYear = entity.ReleaseYear,
        TotalUnits = entity.TotalUnits,
        Genres = entity.Genres,
        AvailableOn = entity.AvailableOn,
        ExternalScore = entity.ExternalScore
    };

    public static MediaItem ToEntity(this MediaItemDto dto) => new()
    {
        ExternalId = dto.ExternalId,
        Source = dto.Source,
        Type = dto.Type,
        Title = dto.Title,
        OriginalTitle = dto.OriginalTitle,
        Synopsis = dto.Synopsis,
        CoverImageUrl = dto.CoverImageUrl,
        BannerImageUrl = dto.BannerImageUrl,
        ReleaseYear = dto.ReleaseYear,
        TotalUnits = dto.TotalUnits,
        Genres = [.. dto.Genres],
        AvailableOn = [.. dto.AvailableOn],
        ExternalScore = dto.ExternalScore,
        LastSyncedAt = DateTime.UtcNow
    };
}