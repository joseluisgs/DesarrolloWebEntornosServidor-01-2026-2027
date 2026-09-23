using Microsoft.EntityFrameworkCore;
using _05_MongoDB_Driver_EFCore.Entity;
using _05_MongoDB_Driver_EFCore.Models;
using _05_MongoDB_Driver_EFCore.Repositories.Base;

namespace _05_MongoDB_Driver_EFCore.Repositories.MongoEfCore;

/// <summary>
/// Implementación del repositorio usando EF Core con MongoDB.
/// EF Core abstrae la base de datos y permite cambiar entre proveedores.
/// </summary>
public class ProductoMongoEfRepository(MongoDbContext context) : IProductoRepository
{
    private readonly MongoDbContext _context = context;

    /// <inheritdoc />
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Productos
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Producto?> GetByIdAsync(string id)
    {
        return await _context.Productos.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Producto> CreateAsync(Producto producto)
    {
        producto.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        producto.CreatedAt = DateTime.UtcNow;
        producto.UpdatedAt = DateTime.UtcNow;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    /// <inheritdoc />
    public async Task<Producto?> UpdateAsync(Producto producto)
    {
        var entity = await _context.Productos.FindAsync(producto.Id);
        if (entity is null) return null;

        entity.Nombre = producto.Nombre;
        entity.Precio = producto.Precio;
        entity.Categoria = producto.Categoria;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return producto;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _context.Productos.FindAsync(id);
        if (entity is null) return false;

        _context.Productos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
