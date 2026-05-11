using MediaHub.Application.Common;
using MediaHub.Application.DTOs;

namespace MediaHub.Application.Interfaces.Services;

public interface ICalendarService
{
    Task<Result<IReadOnlyList<AiringEventDto>>> GetUpcomingAsync(int userId, int days, CancellationToken ct = default);
}