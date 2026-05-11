using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Domain.Enums;

namespace MediaHub.Application.Interfaces.Providers;

/// <summary>
/// Contrat pour une source externe de métadonnées (AniList, TMDB, MangaDex...).
/// Chaque provider gère un ou plusieurs MediaType.
/// </summary>
public interface IMediaProvider
{
    MediaSource Source { get; }
    bool SupportsType(MediaType type);

    Task<Result<MediaSearchResultDto>> SearchAsync(
        string query, MediaType type, int page, int pageSize, CancellationToken ct = default);

    Task<Result<MediaItemDto>> GetByIdAsync(
        string externalId, MediaType type, CancellationToken ct = default);
}