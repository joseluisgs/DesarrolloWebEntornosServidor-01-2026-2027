namespace _06_Redis_Cache.Models;

/// <summary>
/// Modelo de producto con datos básicos de catálogo.
/// </summary>
public record Producto(
    int Id,
    string Nombre,
    decimal Precio,
    string Categoria
);
