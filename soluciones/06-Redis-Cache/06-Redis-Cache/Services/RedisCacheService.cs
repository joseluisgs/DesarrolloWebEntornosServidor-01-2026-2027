using System.Text.Json;
using StackExchange.Redis;

namespace _06_Redis_Cache.Services;

/// <summary>
/// Implementación de ICacheService usando Redis como backend.
/// StackExchange.Redis maneja internamente un pool de conexiones
/// y reconexión automática.
/// </summary>
public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }
}
