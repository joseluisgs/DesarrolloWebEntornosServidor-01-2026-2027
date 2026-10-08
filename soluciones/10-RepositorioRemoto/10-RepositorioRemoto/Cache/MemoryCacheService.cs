using Microsoft.Extensions.Caching.Memory;

namespace _10_RepositorioRemoto.Cache;

/// <summary>
/// Servicio de caché en memoria usando MemoryCache.
/// </summary>
public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private readonly MemoryCache _memoryCache = (MemoryCache)cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };
        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        _memoryCache.Compact(1.0);
        return Task.CompletedTask;
    }
}
