using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Interfaces.Services;

public interface IMediaSearchService
{
    Task<Result<MediaSearchResultDto>> SearchAsync(SearchMediaRequest request, CancellationToken ct = default);
    Task<Result<MediaItemDto>> GetDetailsAsync(string externalId, Domain.Enums.MediaType type, CancellationToken ct = default);
}