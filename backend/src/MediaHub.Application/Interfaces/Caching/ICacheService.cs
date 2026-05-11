namespace MediaHub.Application.Interfaces.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Retourne la valeur en cache, ou exécute le factory et met en cache si absente.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken ct = default) where T : class;
}