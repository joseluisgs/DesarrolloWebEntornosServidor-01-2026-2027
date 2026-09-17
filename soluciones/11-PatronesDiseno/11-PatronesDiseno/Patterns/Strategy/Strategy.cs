// ============================================================
// PATRÓN STRATEGY
// Permite intercambiar algoritmos en runtime.
// Conecta con SOLID: OCP (abiertos a extensión) + DIP (dependen de abstracciones)
// ============================================================

namespace _11_PatronesDiseno.Patterns.Strategy;

/// <summary>
/// Interfaz Strategy — define el contrato para todas las estrategias de validación.
/// </summary>
/// <typeparam name="T">Tipo de entidad a validar.</typeparam>
public interface IValidador<T>
{
    /// <summary>
    /// Valida la entidad y retorna errores si no es válida.
    /// </summary>
    IEnumerable<string> Validar(T entidad);
}

/// <summary>
/// Validador de Persona — Strategy concreta.
/// </summary>
public class PersonaValidador : IValidador<Models.Persona>
{
    /// <inheritdoc />
    public IEnumerable<string> Validar(Models.Persona persona)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(persona.Nombre))
            errores.Add("Nombre requerido");

        if (persona.Nombre.Length < 2)
            errores.Add("Nombre muy corto (mínimo 2 caracteres)");

        if (string.IsNullOrWhiteSpace(persona.Email))
            errores.Add("Email requerido");

        if (!persona.Email.Contains('@'))
            errores.Add("Email inválido");

        if (persona.Edad < 0 || persona.Edad > 150)
            errores.Add("Edad fuera de rango (0-150)");

        return errores;
    }
}

/// <summary>
/// Strategy para ordenar personas — distinto algoritmo según el criterio.
/// </summary>
public interface IOrdenador<T>
{
    IEnumerable<T> Ordenar(IEnumerable<T> elementos);
}

public class OrdenadorPorNombre<T> : IOrdenador<T> where T : Models.Persona
{
    /// <inheritdoc />
    public IEnumerable<T> Ordenar(IEnumerable<T> elementos) =>
        elementos.OrderBy(p => p.Nombre);
}

public class OrdenadorPorEdad<T> : IOrdenador<T> where T : Models.Persona
{
    /// <inheritdoc />
    public IEnumerable<T> Ordenar(IEnumerable<T> elementos) =>
        elementos.OrderBy(p => p.Edad);
}

/// <summary>
/// Servicio que usa Strategy — el algoritmo de validación se inyecta.
/// Demuestra OCP: para añadir un nuevo tipo de validación, solo creas una nueva clase.
/// Demuestra DIP: el servicio depende de IValidador, no de una implementación concreta.
/// </summary>
public class PersonaServiceStrategy(IValidador<Models.Persona> validador)
{
    public (Models.Persona? Persona, IEnumerable<string> Errores) Crear(
        int id, string nombre, string email, int edad)
    {
        var persona = new Models.Persona(id, nombre, email, edad);
        var errores = validador.Validar(persona);

        return errores.Any() ? (null, errores) : (persona, []);
    }
}
