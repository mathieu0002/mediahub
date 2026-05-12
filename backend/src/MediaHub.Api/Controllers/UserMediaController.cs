using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MediaHub.Api.Controllers;

[ApiController]
[Route("api/user-media")]
[Produces("application/json")]
public class UserMediaController : ControllerBase
{
    private readonly IUserMediaService _service;

    // ⚠️ Temporaire : sera remplacé par l'auth à l'Étape 6
    private const int TempUserId = 1;

    public UserMediaController(IUserMediaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Récupère la bibliothèque de l'utilisateur, filtrable par type et statut
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserMediaItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibrary(
        [FromQuery] MediaType? type,
        [FromQuery] WatchStatus? status,
        CancellationToken ct = default)
    {
        var result = await _service.GetLibraryAsync(TempUserId, type, status, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Récupère une entrée précise de la bibliothèque
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserMediaItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(TempUserId, id, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Ajoute une œuvre à la bibliothèque
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserMediaItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] CreateUserMediaRequest request, CancellationToken ct = default)
    {
        var result = await _service.AddAsync(TempUserId, request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Met à jour une entrée (statut, progression, note, notes)
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(UserMediaItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserMediaRequest request, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(TempUserId, id, request, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Supprime une œuvre de la bibliothèque
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int id, CancellationToken ct = default)
    {
        var result = await _service.RemoveAsync(TempUserId, id, ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}