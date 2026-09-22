using _04_PostgreSQL_Dapper_EFCore.Models;

namespace _04_PostgreSQL_Dapper_EFCore.Repositories.Base;

/// <summary>
/// Interfaz base para repositorios de Productos.
/// Define las operaciones CRUD estándar.
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
    Task<Producto?> GetByIdAsync(int id);

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
    Task<bool> DeleteAsync(int id);
}
