using MediaHub.Application.Common;
using MediaHub.Application.DTOs;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Domain.Enums;

namespace MediaHub.Application.Interfaces.Services;

public interface IUserMediaService
{
    Task<Result<UserMediaItemDto>> AddAsync(int userId, CreateUserMediaRequest request, CancellationToken ct = default);
    Task<Result<UserMediaItemDto>> UpdateAsync(int userId, int userMediaId, UpdateUserMediaRequest request, CancellationToken ct = default);
    Task<Result> RemoveAsync(int userId, int userMediaId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<UserMediaItemDto>>> GetLibraryAsync(int userId, MediaType? type, WatchStatus? status, CancellationToken ct = default);
    Task<Result<UserMediaItemDto>> GetByIdAsync(int userId, int userMediaId, CancellationToken ct = default);
}