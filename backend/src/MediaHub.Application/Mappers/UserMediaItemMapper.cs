using MediaHub.Application.DTOs;
using MediaHub.Domain.Entities;

namespace MediaHub.Application.Mappers;

public static class UserMediaItemMapper
{
    public static UserMediaItemDto ToDto(this UserMediaItem entity) => new()
    {
        Id = entity.Id,
        Media = entity.MediaItem.ToDto(),
        Status = entity.Status,
        Progress = entity.Progress,
        UserScore = entity.UserScore,
        Notes = entity.Notes,
        StartedAt = entity.StartedAt,
        CompletedAt = entity.CompletedAt,
        CreatedAt = entity.CreatedAt
    };
}