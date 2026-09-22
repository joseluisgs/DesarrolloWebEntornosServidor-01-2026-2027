using _04_PostgreSQL_Dapper_EFCore.Entity;
using _04_PostgreSQL_Dapper_EFCore.Models;

namespace _04_PostgreSQL_Dapper_EFCore.Mappers;

/// <summary>
/// Funciones de extensión para mapear entre ProductoEntity y Producto.
/// Centraliza la lógica de conversión entre capas.
/// </summary>
public static class ProductoMapper
{
    /// <summary>
    /// Convierte una ProductoEntity a un Producto de dominio.
    /// </summary>
    public static Producto ToModel(this ProductoEntity entity) => new(
        entity.Id,
        entity.Nombre,
        entity.Precio,
        entity.Categoria);

    /// <summary>
    /// Convierte un Producto de dominio a una ProductoEntity para persistencia.
    /// </summary>
    public static ProductoEntity ToEntity(this Producto model) => new()
    {
        Id = model.Id,
        Nombre = model.Nombre,
        Precio = model.Precio,
        Categoria = model.Categoria,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Convierte una colección de ProductoEntity a una lista de Producto.
    /// </summary>
    public static List<Producto> ToModel(this IEnumerable<ProductoEntity> entities)
        => entities.Select(e => e.ToModel()).ToList();
}
