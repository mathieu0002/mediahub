using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Domain.Enums;

namespace MediaHub.Application.Interfaces.Providers;

/// <summary>
/// Contrat pour un provider capable de fournir les prochaines sorties
/// (épisodes d'anime, épisodes de série, sortie de film).
/// </summary>
public interface IUpcomingProvider
{
    MediaSource Source { get; }
    bool SupportsType(MediaType type);

    /// <summary>
    /// Pour une œuvre donnée, retourne les sorties prévues dans la fenêtre temporelle indiquée.
    /// Peut retourner 0, 1 ou plusieurs événements.
    /// </summary>
    Task<Result<IReadOnlyList<AiringEventDto>>> GetUpcomingAsync(
        string externalId,
        MediaType type,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);
}