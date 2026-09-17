// ============================================================
// PATRÓN FACTORY
// Crea objetos sin especificar la clase exacta.
// Conecta con SOLID: SRP (separa creación de uso) + DIP (depende de interfaces)
// ============================================================

namespace _11_PatronesDiseno.Patterns.Factory;

/// <summary>
/// Interfaz del repositorio — lo que el cliente usa.
/// </summary>
public interface IPersonaRepository
{
    void Guardar(Models.Persona persona);
    Models.Persona? Obtener(int id);
    IEnumerable<Models.Persona> ObtenerTodos();
}

/// <summary>
/// Implementación en memoria — para testing y demostración.
/// </summary>
public class PersonaMemoryRepository : IPersonaRepository
{
    private readonly Dictionary<int, Models.Persona> _personas = new();

    /// <inheritdoc />
    public void Guardar(Models.Persona persona) => _personas[persona.Id] = persona;

    /// <inheritdoc />
    public Models.Persona? Obtener(int id) =>
        _personas.TryGetValue(id, out var persona) ? persona : null;

    /// <inheritdoc />
    public IEnumerable<Models.Persona> ObtenerTodos() => _personas.Values;
}

/// <summary>
/// Implementación JSON — persiste en fichero.
/// </summary>
public class PersonaJsonRepository(string rutaFichero) : IPersonaRepository
{
    /// <inheritdoc />
    public void Guardar(Models.Persona persona) =>
        Console.WriteLine($"[JSON] Guardado en {rutaFichero}: {persona.Nombre}");

    /// <inheritdoc />
    public Models.Persona? Obtener(int id) =>
        Console.ReadLine() is not null ? new Models.Persona(id, "Ana", "ana@email.com", 25) : null;

    /// <inheritdoc />
    public IEnumerable<Models.Persona> ObtenerTodos() =>
        [new(1, "Ana", "ana@email.com", 25), new(2, "Bob", "bob@email.com", 30)];
}

/// <summary>
/// Factory — crea repositorios según la configuración.
/// SRP: Solo se encarga de crear objetos.
/// </summary>
public static class PersonaRepositoryFactory
{
    /// <summary>
    /// Crea un repositorio según el tipo indicado.
    /// </summary>
    /// <param name="tipo">Tipo de repositorio: "Memory" o "Json".</param>
    /// <returns>Implementación de IPersonaRepository.</returns>
    public static IPersonaRepository Crear(string tipo) => tipo.ToLower() switch
    {
        "memory" => new PersonaMemoryRepository(),
        "json" => new PersonaJsonRepository("data/personas.json"),
        _ => throw new ArgumentException($"Tipo de repositorio no soportado: {tipo}")
    };
}
