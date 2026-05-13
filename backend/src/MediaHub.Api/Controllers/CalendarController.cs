using MediaHub.Application.DTOs;
using MediaHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaHub.Api.Controllers;

[ApiController]
[Route("api/calendar")]
[Authorize]
[Produces("application/json")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _service;
    private readonly ICurrentUserService _currentUser;

    public CalendarController(ICalendarService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retourne les sorties à venir pour les œuvres en cours dans la bibliothèque de l'utilisateur.
    /// </summary>
    /// <param name="days">Nombre de jours à regarder dans le futur (défaut 7, max 60)</param>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IReadOnlyList<AiringEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var result = await _service.GetUpcomingAsync(userId, days, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}