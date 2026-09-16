using ProductoApp.Interfaces;
using ProductoApp.Models;
using ProductoApp.Repositories.Base;
using ProductoApp.Services.Base;

namespace ProductoApp.Services;

/// <summary>
/// Implementación del servicio de productos con Inyección de Dependencias.
/// </summary>
/// <param name="repository">Repositorio de productos inyectado.</param>
public class ProductoService(IProductoRepository repository) : IProductoService, IScopedService
{
    /// <inheritdoc />
    public IEnumerable<Producto> GetAll() => repository.GetAll();

    /// <inheritdoc />
    public Producto? GetById(int id) => repository.GetById(id);

    /// <inheritdoc />
    public Producto Create(Producto producto) => repository.Create(producto);
}
