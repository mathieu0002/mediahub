using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHub.Application.Services;

public class CalendarService : ICalendarService
{
    private readonly IAppDbContext _db;
    private readonly IEnumerable<IUpcomingProvider> _providers;
    private readonly ICacheService _cache;
    private readonly ILogger<CalendarService> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public CalendarService(
        IAppDbContext db,
        IEnumerable<IUpcomingProvider> providers,
        ICacheService cache,
        ILogger<CalendarService> logger)
    {
        _db = db;
        _providers = providers;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<AiringEventDto>>> GetUpcomingAsync(
        int userId, int days, CancellationToken ct = default)
    {
        if (days <= 0 || days > 60)
            return Result.Failure<IReadOnlyList<AiringEventDto>>("days doit être entre 1 et 60");

        var cacheKey = $"calendar:user:{userId}:days:{days}";

        var cached = await _cache.GetAsync<List<AiringEventDto>>(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Calendar cache hit for user {UserId}", userId);
            return Result.Success<IReadOnlyList<AiringEventDto>>(cached);
        }

        // 1. Récup les œuvres en cours du user
        var watching = await _db.UserMediaItems
            .Include(um => um.MediaItem)
            .Where(um => um.UserId == userId && um.Status == WatchStatus.Watching)
            .Select(um => new { um.MediaItem.ExternalId, um.MediaItem.Type, um.MediaItem.Source })
            .ToListAsync(ct);

        if (watching.Count == 0)
            return Result.Success<IReadOnlyList<AiringEventDto>>([]);

        // 2. Fenêtre temporelle
        var fromUtc = DateTime.UtcNow;
        var toUtc = fromUtc.AddDays(days);

        // 3. Appels en parallèle pour chaque œuvre (avec limite raisonnable)
        var allEvents = new List<AiringEventDto>();
        var lockObj = new object();

        await Parallel.ForEachAsync(
            watching,
            new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct },
            async (item, innerCt) =>
            {
                var provider = _providers.FirstOrDefault(p =>
                    p.Source == item.Source && p.SupportsType(item.Type));

                if (provider is null) return;

                var result = await provider.GetUpcomingAsync(
                    item.ExternalId, item.Type, fromUtc, toUtc, innerCt);

                if (result.IsSuccess && result.Value is not null)
                {
                    lock (lockObj)
                    {
                        allEvents.AddRange(result.Value);
                    }
                }
            });

        var sorted = allEvents
            .OrderBy(e => e.AiringAt)
            .ToList();

        await _cache.SetAsync(cacheKey, sorted, CacheTtl, ct);
        return Result.Success<IReadOnlyList<AiringEventDto>>(sorted);
    }
}