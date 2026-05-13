using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Application.Mappers;
using MediaHub.Domain.Entities;
using MediaHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHub.Application.Services;

public class UserMediaService : IUserMediaService
{
    private readonly IAppDbContext _db;
    private readonly IMediaSearchService _searchService;
    private readonly ICacheService _cache;
    private readonly ILogger<UserMediaService> _logger;

    public UserMediaService(
        IAppDbContext db,
        IMediaSearchService searchService,
        ICacheService cache,
        ILogger<UserMediaService> logger)
    {
        _db = db;
        _searchService = searchService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<UserMediaItemDto>> AddAsync(
        int userId, CreateUserMediaRequest request, CancellationToken ct = default)
    {
        // 1. Trouve ou crée le MediaItem en base
        var mediaItem = await _db.MediaItems
            .FirstOrDefaultAsync(m =>
                m.Source == request.Source &&
                m.ExternalId == request.ExternalId, ct);

        if (mediaItem is null)
        {
            // Récup depuis l'API externe
            var detailsResult = await _searchService.GetDetailsAsync(request.ExternalId, request.Type, ct);
            if (detailsResult.IsFailure)
                return Result.Failure<UserMediaItemDto>(detailsResult.Error!);

            mediaItem = detailsResult.Value!.ToEntity();
            _db.MediaItems.Add(mediaItem);
            await _db.SaveChangesAsync(ct);
        }

        // 2. Vérifie qu'il n'est pas déjà dans la lib
        var existing = await _db.UserMediaItems
            .FirstOrDefaultAsync(u => u.UserId == userId && u.MediaItemId == mediaItem.Id, ct);

        if (existing is not null)
            return Result.Failure<UserMediaItemDto>("Déjà dans votre bibliothèque");

        // 3. Ajoute à la lib user
        var userMedia = new UserMediaItem
        {
            UserId = userId,
            MediaItemId = mediaItem.Id,
            Status = request.Status,
            StartedAt = request.Status == WatchStatus.Watching ? DateTime.UtcNow : null
        };

        _db.UserMediaItems.Add(userMedia);
        await _db.SaveChangesAsync(ct);

        userMedia.MediaItem = mediaItem;
        await InvalidateCalendarCacheAsync(userId, ct);
        return Result.Success(userMedia.ToDto());
    }

    public async Task<Result<UserMediaItemDto>> UpdateAsync(
        int userId, int userMediaId, UpdateUserMediaRequest request, CancellationToken ct = default)
    {
        var entity = await _db.UserMediaItems
            .Include(u => u.MediaItem)
            .FirstOrDefaultAsync(u => u.Id == userMediaId && u.UserId == userId, ct);

        if (entity is null)
            return Result.Failure<UserMediaItemDto>("Entrée introuvable");

        if (request.Status.HasValue)
        {
            entity.Status = request.Status.Value;
            if (request.Status == WatchStatus.Completed && entity.CompletedAt is null)
                entity.CompletedAt = DateTime.UtcNow;
            if (request.Status == WatchStatus.Watching && entity.StartedAt is null)
                entity.StartedAt = DateTime.UtcNow;
        }

        if (request.Progress.HasValue) entity.Progress = request.Progress.Value;
        if (request.UserScore.HasValue) entity.UserScore = request.UserScore.Value;
        if (request.Notes is not null) entity.Notes = request.Notes;

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await InvalidateCalendarCacheAsync(userId, ct);
        return Result.Success(entity.ToDto());
    }

    public async Task<Result> RemoveAsync(int userId, int userMediaId, CancellationToken ct = default)
    {
        var entity = await _db.UserMediaItems
            .FirstOrDefaultAsync(u => u.Id == userMediaId && u.UserId == userId, ct);

        if (entity is null)
            return Result.Failure("Entrée introuvable");

        _db.UserMediaItems.Remove(entity);
        await _db.SaveChangesAsync(ct);
        await InvalidateCalendarCacheAsync(userId, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<UserMediaItemDto>>> GetLibraryAsync(
        int userId, MediaType? type, WatchStatus? status, CancellationToken ct = default)
    {
        var query = _db.UserMediaItems
            .Include(u => u.MediaItem)
            .Where(u => u.UserId == userId);

        if (type.HasValue) query = query.Where(u => u.MediaItem.Type == type.Value);
        if (status.HasValue) query = query.Where(u => u.Status == status.Value);

        var items = await query
            .OrderByDescending(u => u.UpdatedAt ?? u.CreatedAt)
            .ToListAsync(ct);

        IReadOnlyList<UserMediaItemDto> dtos = items.Select(u => u.ToDto()).ToList();
        return Result.Success(dtos);
    }

    public async Task<Result<UserMediaItemDto>> GetByIdAsync(int userId, int userMediaId, CancellationToken ct = default)
    {
        var entity = await _db.UserMediaItems
            .Include(u => u.MediaItem)
            .FirstOrDefaultAsync(u => u.Id == userMediaId && u.UserId == userId, ct);

        return entity is null
            ? Result.Failure<UserMediaItemDto>("Entrée introuvable")
            : Result.Success(entity.ToDto());
    }
    
    private async Task InvalidateCalendarCacheAsync(int userId, CancellationToken ct)
    {
        for (var days = 1; days <= 60; days++)
        {
            await _cache.RemoveAsync($"calendar:user:{userId}:days:{days}", ct);
        }
    }
}