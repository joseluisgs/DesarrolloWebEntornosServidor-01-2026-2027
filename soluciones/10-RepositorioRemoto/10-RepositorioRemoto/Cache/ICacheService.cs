namespace _10_RepositorioRemoto.Cache;

/// <summary>
/// Interfaz para el servicio de caché.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
}
