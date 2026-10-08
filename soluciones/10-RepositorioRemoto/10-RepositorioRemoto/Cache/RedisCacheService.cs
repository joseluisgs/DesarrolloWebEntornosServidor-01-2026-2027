using System.Text.Json;
using StackExchange.Redis;

namespace _10_RepositorioRemoto.Cache;

/// <summary>
/// Implementación de <see cref="ICacheService"/> usando Redis como backend (producción).
/// StackExchange.Redis mantiene internamente un pool de conexiones y reconexión automática.
/// Los valores se serializan a JSON, así que sirve para cualquier tipo.
/// </summary>
public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        // Redis no tiene un "FLUSHDB" explícito en IDatabase: recorremos las claves
        // del servidor y las borramos. En producción usaríamos prefijos por namespace.
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var keys = server.Keys(pattern: "*").ToArray();
            if (keys.Length == 0)
            {
                continue;
            }

            await _db.KeyDeleteAsync(keys);
        }
    }
}
