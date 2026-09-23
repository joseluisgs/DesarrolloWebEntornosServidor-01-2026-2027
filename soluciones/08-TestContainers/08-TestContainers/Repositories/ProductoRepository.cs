using Microsoft.EntityFrameworkCore;
using _08_TestContainers.Entity;
using _08_TestContainers.Models;

namespace _08_TestContainers.Repositories;

/// <summary>
/// Repositorio de productos usando Entity Framework Core con PostgreSQL.
/// Gestiona las operaciones CRUD entre el dominio y la base de datos.
/// </summary>
public class ProductoRepository(AppDbContext context)
{
    /// <summary>
    /// Obtiene todos los productos de la base de datos.
    /// </summary>
    public async Task<List<Producto>> GetAllAsync()
    {
        var entities = await context.Productos
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return entities.Select(e => new Producto(
            e.Id,
            e.Nombre,
            e.Precio,
            e.Categoria
        )).ToList();
    }

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    public async Task<Producto?> GetByIdAsync(int id)
    {
        var entity = await context.Productos.FindAsync(id);
        return entity is null
            ? null
            : new Producto(entity.Id, entity.Nombre, entity.Precio, entity.Categoria);
    }

    /// <summary>
    /// Crea un nuevo producto en la base de datos.
    /// </summary>
    public async Task<Producto> CreateAsync(Producto producto)
    {
        var entity = new ProductoEntity
        {
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Categoria = producto.Categoria
        };

        context.Productos.Add(entity);
        await context.SaveChangesAsync();

        return new Producto(entity.Id, entity.Nombre, entity.Precio, entity.Categoria);
    }

    /// <summary>
    /// Elimina un producto por su ID.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await context.Productos.FindAsync(id);
        if (entity is null)
        {
            return false;
        }

        context.Productos.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
