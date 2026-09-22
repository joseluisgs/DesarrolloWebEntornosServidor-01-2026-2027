using Microsoft.EntityFrameworkCore;
using _04_PostgreSQL_Dapper_EFCore.Entity;
using _04_PostgreSQL_Dapper_EFCore.Mappers;
using _04_PostgreSQL_Dapper_EFCore.Models;
using _04_PostgreSQL_Dapper_EFCore.Repositories.Base;

namespace _04_PostgreSQL_Dapper_EFCore.Repositories.EfCore;

/// <summary>
/// Implementación del repositorio usando Entity Framework Core.
/// EF Core es un ORM completo con change tracking y migraciones.
/// </summary>
public class ProductoEfRepository(AppDbContext context) : IProductoRepository
{
    private readonly AppDbContext _context = context;


    /// <inheritdoc />
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        var entities = await _context.Productos
            .OrderBy(p => p.Id)
            .ToListAsync();

        return entities.ToModel();
    }

    /// <inheritdoc />
    public async Task<Producto?> GetByIdAsync(int id)
    {
        var entity = await _context.Productos.FindAsync(id);
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public async Task<Producto> CreateAsync(Producto producto)
    {
        var entity = producto.ToEntity();

        _context.Productos.Add(entity);
        await _context.SaveChangesAsync();

        return producto with { Id = entity.Id };
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
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Productos.FindAsync(id);
        if (entity is null) return false;

        _context.Productos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
