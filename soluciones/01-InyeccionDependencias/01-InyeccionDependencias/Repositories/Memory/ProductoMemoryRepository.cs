using ProductoApp.Interfaces;
using ProductoApp.Models;
using ProductoApp.Repositories.Base;

namespace ProductoApp.Repositories.Memory;

/// <summary>
///     Implementación del repositorio de productos en memoria.
/// </summary>
public class ProductoMemoryRepository : IProductoRepository, ITransientService {
    private readonly List<Producto> _productos = [
        new(1, "Portátil", 999.99m),
        new(2, "Ratón", 29.99m),
        new(3, "Teclado", 59.99m)
    ];

    private int _nextId = 4;

    /// <inheritdoc />
    public IEnumerable<Producto> GetAll() {
        return _productos.AsReadOnly();
    }

    /// <inheritdoc />
    public Producto? GetById(int id) {
        return _productos.FirstOrDefault(p => p.Id == id);
    }

    /// <inheritdoc />
    public Producto Create(Producto producto) {
        var nuevoProducto = producto with { Id = _nextId++ };
        _productos.Add(nuevoProducto);
        return nuevoProducto;
    }
}