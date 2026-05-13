using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaHub.Api.Controllers;

[ApiController]
[Route("api/user-media")]
[Authorize]
[Produces("application/json")]
public class UserMediaController : ControllerBase
{
    private readonly IUserMediaService _service;
    private readonly ICurrentUserService _currentUser;

    public UserMediaController(IUserMediaService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private int UserId => _currentUser.UserId
        ?? throw new UnauthorizedAccessException("User non authentifié");

    [HttpGet]
    public async Task<IActionResult> GetLibrary(
        [FromQuery] MediaType? type,
        [FromQuery] WatchStatus? status,
        CancellationToken ct = default)
    {
        var result = await _service.GetLibraryAsync(UserId, type, status, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(UserId, id, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateUserMediaRequest request, CancellationToken ct = default)
    {
        var result = await _service.AddAsync(UserId, request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserMediaRequest request, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(UserId, id, request, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct = default)
    {
        var result = await _service.RemoveAsync(UserId, id, ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}