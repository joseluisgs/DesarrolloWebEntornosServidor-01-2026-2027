using ProductoApp.Models;

namespace ProductoApp.Repositories.Base;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de productos.
/// </summary>
public interface IProductoRepository
{
    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    IEnumerable<Producto> GetAll();

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    Producto? GetById(int id);

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    Producto Create(Producto producto);
}
