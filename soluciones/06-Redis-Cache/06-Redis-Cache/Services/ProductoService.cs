using _06_Redis_Cache.Models;

namespace _06_Redis_Cache.Services;

/// <summary>
/// Servicio de productos que implementa el patrón Cache-Aside.
/// Flujo: cache hit → devuelve de Redis | cache miss → consulta BD → guarda en cache.
/// </summary>
public class ProductoService(ICacheService cache) : IProductoService
{
    private const string CachePrefix = "producto:";

    /// <summary>
    /// Simula una base de datos de productos con datos iniciales.
    /// </summary>
    private static readonly List<Producto> ProductosDb =
    [
        new(1, "Portátil Dell XPS 15", 1299.99m, "Informática"),
        new(2, "Monitor LG UltraWide 34\"", 449.99m, "Periféricos"),
        new(3, "Teclado Mecánico Logitech", 89.99m, "Periféricos"),
        new(4, "Ratón Gaming Razer", 69.99m, "Periféricos"),
        new(5, "Disco SSD Samsung 1TB", 109.99m, "Almacenamiento"),
    ];

    /// <summary>
    /// Busca un producto por ID. Primero consulta la caché;
    /// si no está, accede a la "base de datos" y guarda en caché.
    /// </summary>
    public async Task<Producto?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CachePrefix}{id}";

        // Paso 1: Intentar obtener de la caché (cache hit)
        var cached = await cache.GetAsync<Producto>(cacheKey);
        if (cached is not null)
        {
            Console.WriteLine($"  [CACHE HIT] Producto {id} encontrado en Redis");
            return cached;
        }

        Console.WriteLine($"  [CACHE MISS] Producto {id} no está en Redis, consultando BD...");

        // Paso 2: Si no está en caché, ir a la "base de datos"
        await SimularLatenciaBaseDatos();
        var producto = ProductosDb.FirstOrDefault(p => p.Id == id);

        // Paso 3: Guardar en caché para futuras peticiones
        if (producto is not null)
        {
            await cache.SetAsync(cacheKey, producto, TimeSpan.FromMinutes(30));
            Console.WriteLine($"  [CACHE SET] Producto {id} guardado en Redis (TTL: 30 min)");
        }

        return producto;
    }

    /// <summary>
    /// Obtiene todos los productos. Al ser una lista pequeña, se cachea
    /// la lista completa con una clave compuesta.
    /// </summary>
    public async Task<List<Producto>> GetAllAsync()
    {
        const string cacheKey = "producto:all";

        var cached = await cache.GetAsync<List<Producto>>(cacheKey);
        if (cached is not null)
        {
            Console.WriteLine("  [CACHE HIT] Lista de productos en Redis");
            return cached;
        }

        Console.WriteLine("  [CACHE MISS] Consultando BD para obtener todos los productos...");
        await SimularLatenciaBaseDatos();

        var productos = ProductosDb.ToList();
        await cache.SetAsync(cacheKey, productos, TimeSpan.FromMinutes(10));

        return productos;
    }

    /// <summary>
    /// Actualiza un producto y invalida las entradas de caché relacionadas.
    /// </summary>
    public async Task<Producto?> UpdateAsync(int id, decimal nuevoPrecio)
    {
        await SimularLatenciaBaseDatos();
        var producto = ProductosDb.FirstOrDefault(p => p.Id == id);
        if (producto is null)
        {
            return null;
        }

        // Actualizar en "BD"
        var index = ProductosDb.IndexOf(producto);
        producto = producto with { Precio = nuevoPrecio };
        ProductosDb[index] = producto;

        // Invalidar caché
        await cache.RemoveAsync($"{CachePrefix}{id}");
        await cache.RemoveAsync("producto:all");
        Console.WriteLine($"  [CACHE INVALIDATE] Caché del producto {id} y lista invalidada");

        return producto;
    }

    /// <summary>
    /// Simula la latencia de una base de datos real (~50ms).
    /// </summary>
    private static Task SimularLatenciaBaseDatos()
    {
        return Task.Delay(50);
    }
}
