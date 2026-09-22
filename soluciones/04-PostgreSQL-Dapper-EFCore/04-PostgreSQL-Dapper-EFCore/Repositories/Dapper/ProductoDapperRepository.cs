using Dapper;
using Npgsql;
using _04_PostgreSQL_Dapper_EFCore.Entity;
using _04_PostgreSQL_Dapper_EFCore.Mappers;
using _04_PostgreSQL_Dapper_EFCore.Models;
using _04_PostgreSQL_Dapper_EFCore.Repositories.Base;

namespace _04_PostgreSQL_Dapper_EFCore.Repositories.Dapper;

/// <summary>
/// Implementación del repositorio usando Dapper (micro ORM).
/// Dapper mapea directamente SQL a objetos C#.
/// </summary>
public class ProductoDapperRepository(string connectionString) : IProductoRepository
{
    private readonly string _connectionString = connectionString;

    /// <inheritdoc />
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = "SELECT id, nombre, precio, categoria FROM productos ORDER BY id";
        var entities = await connection.QueryAsync<ProductoEntity>(sql);
        return entities.ToModel();
    }

    /// <inheritdoc />
    public async Task<Producto?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = "SELECT id, nombre, precio, categoria FROM productos WHERE id = @Id";
        var entity = await connection.QuerySingleOrDefaultAsync<ProductoEntity>(sql, new { Id = id });
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public async Task<Producto> CreateAsync(Producto producto)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = """
            INSERT INTO productos (nombre, precio, categoria, created_at, updated_at)
            VALUES (@Nombre, @Precio, @Categoria, @CreatedAt, @UpdatedAt)
            RETURNING id
            """;

        var entity = producto.ToEntity();

        var id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return producto with { Id = id };
    }

    /// <inheritdoc />
    public async Task<Producto?> UpdateAsync(Producto producto)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = """
            UPDATE productos
            SET nombre = @Nombre, precio = @Precio, categoria = @Categoria, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        var rows = await connection.ExecuteAsync(sql, new
        {
            producto.Id,
            producto.Nombre,
            producto.Precio,
            producto.Categoria,
            UpdatedAt = DateTime.UtcNow
        });

        return rows > 0 ? producto : null;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = "DELETE FROM productos WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
