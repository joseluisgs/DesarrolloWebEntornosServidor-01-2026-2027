namespace _08_TestContainers.Models;

/// <summary>
/// Modelo de producto para el dominio de la aplicación.
/// </summary>
public record Producto(
    int Id,
    string Nombre,
    decimal Precio,
    string Categoria
);
