using _05_MongoDB_Driver_EFCore.Models;

namespace _05_MongoDB_Driver_EFCore.Repositories.Base;

/// <summary>
/// Interfaz base para repositorios de Productos en MongoDB.
/// </summary>
public interface IProductoRepository
{
    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    Task<IEnumerable<Producto>> GetAllAsync();

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    Task<Producto?> GetByIdAsync(string id);

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    Task<Producto> CreateAsync(Producto producto);

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    Task<Producto?> UpdateAsync(Producto producto);

    /// <summary>
    /// Elimina un producto por su ID.
    /// </summary>
    Task<bool> DeleteAsync(string id);
}
