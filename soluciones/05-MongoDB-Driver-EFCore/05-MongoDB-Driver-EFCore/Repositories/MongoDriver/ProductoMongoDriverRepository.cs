using MongoDB.Driver;
using _05_MongoDB_Driver_EFCore.Models;
using _05_MongoDB_Driver_EFCore.Repositories.Base;

namespace _05_MongoDB_Driver_EFCore.Repositories.MongoDriver;

/// <summary>
/// Implementación del repositorio usando el driver nativo de MongoDB.
/// Ofrece control total sobre las consultas y operaciones.
/// </summary>
public class ProductoMongoDriverRepository : IProductoRepository
{
    private readonly IMongoCollection<Producto> _collection;

    /// <summary>
    /// Constructor con cadena de conexión y nombre de base de datos.
    /// </summary>
    public ProductoMongoDriverRepository(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Producto>("productos");
    }

    /// <summary>
    /// Constructor para testing con IMongoClient inyectado.
    /// </summary>
    public ProductoMongoDriverRepository(IMongoClient client, string databaseName)
    {
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Producto>("productos");
    }

    /// <summary>
    /// Constructor con base de datos pre-configurada.
    /// </summary>
    public ProductoMongoDriverRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Producto>("productos");
    }

    /// <summary>
    /// Constructor por defecto con conexión local.
    /// </summary>
    public ProductoMongoDriverRepository()
        : this("mongodb://localhost:27017", "productos_db")
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        var filter = FilterDefinition<Producto>.Empty;
        var sort = Builders<Producto>.Sort.Ascending(p => p.Nombre);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Producto?> GetByIdAsync(string id)
    {
        var filter = Builders<Producto>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<Producto> CreateAsync(Producto producto)
    {
        producto.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        producto.CreatedAt = DateTime.UtcNow;
        producto.UpdatedAt = DateTime.UtcNow;

        await _collection.InsertOneAsync(producto);
        return producto;
    }

    /// <inheritdoc />
    public async Task<Producto?> UpdateAsync(Producto producto)
    {
        producto.UpdatedAt = DateTime.UtcNow;

        var filter = Builders<Producto>.Filter.Eq(p => p.Id, producto.Id);
        var result = await _collection.ReplaceOneAsync(filter, producto);

        return result.MatchedCount > 0 ? producto : null;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<Producto>.Filter.Eq(p => p.Id, id);
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}
