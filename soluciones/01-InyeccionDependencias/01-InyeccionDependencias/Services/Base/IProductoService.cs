using ProductoApp.Models;

namespace ProductoApp.Services.Base;

/// <summary>
/// Interfaz que define las operaciones de negocio de productos.
/// </summary>
public interface IProductoService
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
