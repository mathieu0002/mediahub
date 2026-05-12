using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MediaHub.Api.Controllers;

[ApiController]
[Route("api/media")]
[Produces("application/json")]
public class MediaController : ControllerBase
{
    private readonly IMediaSearchService _searchService;

    public MediaController(IMediaSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Recherche d'œuvres dans les sources externes
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(MediaSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] MediaType type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var request = new SearchMediaRequest
        {
            Query = q,
            Type = type,
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchService.SearchAsync(request, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Détails d'une œuvre depuis sa source externe
    /// </summary>
    [HttpGet("{type}/{externalId}")]
    [ProducesResponseType(typeof(MediaItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(
        MediaType type,
        string externalId,
        CancellationToken ct = default)
    {
        var result = await _searchService.GetDetailsAsync(externalId, type, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }
}