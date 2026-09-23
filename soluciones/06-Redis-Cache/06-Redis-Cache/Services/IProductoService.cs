using _06_Redis_Cache.Models;

namespace _06_Redis_Cache.Services;

/// <summary>
/// Interfaz del servicio de productos con operaciones Cache-Aside.
/// </summary>
public interface IProductoService
{
    /// <summary>
    /// Obtiene un producto por ID usando Cache-Aside.
    /// </summary>
    Task<Producto?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    Task<List<Producto>> GetAllAsync();

    /// <summary>
    /// Actualiza el precio de un producto y invalida la caché.
    /// </summary>
    Task<Producto?> UpdateAsync(int id, decimal nuevoPrecio);
}
