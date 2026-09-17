namespace _03_FicherosJsonCsv.Models;

/// <summary>
/// Representa un producto con sus propiedades básicas.
/// </summary>
public record Producto(
    int Id,
    string Nombre,
    decimal Precio,
    string Categoria
);
