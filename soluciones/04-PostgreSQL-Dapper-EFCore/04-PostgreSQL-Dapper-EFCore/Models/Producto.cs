namespace _04_PostgreSQL_Dapper_EFCore.Models;

/// <summary>
/// Modelo de dominio para Productos.
/// </summary>
public record Producto(
    int Id,
    string Nombre,
    decimal Precio,
    string Categoria
);
