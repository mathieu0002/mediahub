using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MediaHub.Application.Services;

public class MediaSearchService : IMediaSearchService
{
    private readonly IEnumerable<IMediaProvider> _providers;
    private readonly ICacheService _cache;
    private readonly ILogger<MediaSearchService> _logger;

    private static readonly TimeSpan SearchCacheTtl = TimeSpan.FromHours(6);
    private static readonly TimeSpan DetailsCacheTtl = TimeSpan.FromHours(24);

    public MediaSearchService(
        IEnumerable<IMediaProvider> providers,
        ICacheService cache,
        ILogger<MediaSearchService> logger)
    {
        _providers = providers;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<MediaSearchResultDto>> SearchAsync(
        SearchMediaRequest request, CancellationToken ct = default)
    {
        var provider = _providers.FirstOrDefault(p => p.SupportsType(request.Type));
        if (provider is null)
        {
            _logger.LogWarning("No provider supports type {Type}", request.Type);
            return Result.Failure<MediaSearchResultDto>($"Aucun provider pour le type {request.Type}");
        }

        var cacheKey = $"search:{request.Type}:{request.Query.ToLowerInvariant()}:p{request.Page}:s{request.PageSize}";

        var cached = await _cache.GetAsync<MediaSearchResultDto>(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {Key}", cacheKey);
            return Result.Success(cached);
        }

        var result = await provider.SearchAsync(request.Query, request.Type, request.Page, request.PageSize, ct);

        if (result.IsSuccess && result.Value is not null)
        {
            await _cache.SetAsync(cacheKey, result.Value, SearchCacheTtl, ct);
        }

        return result;
    }

    public async Task<Result<MediaItemDto>> GetDetailsAsync(
        string externalId, MediaType type, CancellationToken ct = default)
    {
        var provider = _providers.FirstOrDefault(p => p.SupportsType(type));
        if (provider is null)
            return Result.Failure<MediaItemDto>($"Aucun provider pour le type {type}");

        var cacheKey = $"details:{type}:{externalId}";

        return await _cache.GetOrSetAsync(cacheKey, async (innerCt) =>
        {
            var result = await provider.GetByIdAsync(externalId, type, innerCt);
            return result.Value!;
        }, DetailsCacheTtl, ct) is { } value
            ? Result.Success(value)
            : Result.Failure<MediaItemDto>("Introuvable");
    }
}