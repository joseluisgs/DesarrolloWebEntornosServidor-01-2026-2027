// ============================================================
// PATRÓN DECORATOR
// Añade comportamiento a un objeto sin modificar su código.
// Conecta con SOLID: OCP (extiende sin modificar) + SRP (cada decorator hace una cosa)
// Ejemplos reales: logging, caché, retry, métricas
// ============================================================

namespace _11_PatronesDiseno.Patterns.Decorator;

/// <summary>
/// Interfaz base — el objeto que se va a decorar.
/// </summary>
public interface IRepository<T>
{
    T? Obtener(int id);
    void Guardar(T entity);
}

/// <summary>
/// Implementación base — repositorio simple en memoria.
/// </summary>
public class InMemoryRepository<T> : IRepository<T>
{
    private readonly Dictionary<int, T> _store = new();

    /// <inheritdoc />
    public T? Obtener(int id) =>
        _store.TryGetValue(id, out var entity) ? entity : default;

    /// <inheritdoc />
    public void Guardar(T entity) =>
        _store[entity!.GetHashCode()] = entity;

    /// <summary>
    /// Guarda con un ID específico (para el ejemplo).
    /// </summary>
    public void GuardarConId(int id, T entity) => _store[id] = entity;
}

/// <summary>
/// Decorator de Logging — añade logs antes y después de cada operación.
/// No modifica el repositorio original, solo le añade comportamiento.
/// </summary>
public class LoggingRepositoryDecorator<T>(IRepository<T> inner) : IRepository<T>
{
    /// <inheritdoc />
    public T? Obtener(int id)
    {
        Console.WriteLine($"[LOG] Antes de Obtener({id})");
        var resultado = inner.Obtener(id);
        Console.WriteLine($"[LOG] Después de Obtener({id}): {(resultado is not null ? "encontrado" : "no encontrado")}");
        return resultado;
    }

    /// <inheritdoc />
    public void Guardar(T entity)
    {
        Console.WriteLine($"[LOG] Antes de Guardar");
        inner.Guardar(entity);
        Console.WriteLine($"[LOG] Después de Guardar");
    }
}

/// <summary>
/// Decorator de Métricas — cuenta las operaciones.
/// Se puede combinar con LoggingDecorator: Logging(Metrics(Repository))
/// </summary>
public class MetricsRepositoryDecorator<T>(IRepository<T> inner) : IRepository<T>
{
    private int _obtenerCalls;
    private int _guardarCalls;

    /// <inheritdoc />
    public T? Obtener(int id)
    {
        _obtenerCalls++;
        Console.WriteLine($"[METRICS] Obtener llamado {_obtenerCalls} veces");
        return inner.Obtener(id);
    }

    /// <inheritdoc />
    public void Guardar(T entity)
    {
        _guardarCalls++;
        Console.WriteLine($"[METRICS] Guardar llamado {_guardarCalls} veces");
        inner.Guardar(entity);
    }
}

/// <summary>
/// Decorator de Retry — reintenta la operación si falla.
/// </summary>
public class RetryRepositoryDecorator<T>(IRepository<T> inner, int maxRetries = 3) : IRepository<T>
{
    /// <inheritdoc />
    public T? Obtener(int id)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return inner.Obtener(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RETRY] Intento {i + 1}/{maxRetries} falló: {ex.Message}");
            }
        }
        return default;
    }

    /// <inheritdoc />
    public void Guardar(T entity)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                inner.Guardar(entity);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RETRY] Intento {i + 1}/{maxRetries} falló: {ex.Message}");
            }
        }
    }
}
