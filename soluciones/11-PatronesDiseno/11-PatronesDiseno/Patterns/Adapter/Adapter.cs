// ============================================================
// PATRÓN ADAPTER
// Convierte la interfaz de una clase en otra que el cliente espera.
// Conecta con SOLID: DIP (el cliente depende de abstracciones)
// Ejemplo real: Refit es un adapter, EF Core adapta LINQ a SQL
// ============================================================

namespace _11_PatronesDiseno.Patterns.Adapter;

/// <summary>
/// Interfaz que el cliente espera — modelo propio de la app.
/// </summary>
public interface IUsuarioService
{
    UsuarioDto? Obtener(int id);
    IEnumerable<UsuarioDto> ObtenerTodos();
}

public record UsuarioDto(int Id, string NombreCompleto, string Email);

/// <summary>
/// Librería de terceros con interfaz diferente (simulada).
/// En la vida real, esto sería una API externa, un SDK, etc.
/// </summary>
public class ApiExternaUsuario
{
    // La API externa devuelve un formato diferente
    public ExternaUsuarioResponse? FetchUser(int id) => id switch
    {
        1 => new() { UserId = 1, FirstName = "Ana", LastName = "García", Mail = "ana@email.com" },
        2 => new() { UserId = 2, FirstName = "Bob", LastName = "López", Mail = "bob@email.com" },
        _ => null
    };

    public IEnumerable<ExternaUsuarioResponse> FetchAllUsers() =>
    [
        new() { UserId = 1, FirstName = "Ana", LastName = "García", Mail = "ana@email.com" },
        new() { UserId = 2, FirstName = "Bob", LastName = "López", Mail = "bob@email.com" }
    ];
}

public record ExternaUsuarioResponse
{
    public int UserId { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Mail { get; init; } = "";
}

/// <summary>
/// Adapter — convierte la interfaz de la API externa a nuestra interfaz.
/// El cliente (UsuarioService) no sabe que está usando una API externa.
/// </summary>
public class ApiExternaAdapter(ApiExternaUsuario api) : IUsuarioService
{
    /// <inheritdoc />
    public UsuarioDto? Obtener(int id)
    {
        var response = api.FetchUser(id);
        return response is null ? null : MapToDto(response);
    }

    /// <inheritdoc />
    public IEnumerable<UsuarioDto> ObtenerTodos() =>
        api.FetchAllUsers().Select(MapToDto);

    private static UsuarioDto MapToDto(ExternaUsuarioResponse response) =>
        new(response.UserId, $"{response.FirstName} {response.LastName}", response.Mail);
}

/// <summary>
/// Servicio que usa la interfaz IUsuarioService — no sabe que hay un Adapter.
/// </summary>
public class UsuarioService(IUsuarioService repository)
{
    public void Mostrar(int id)
    {
        var usuario = repository.Obtener(id);
        Console.WriteLine(usuario is not null
            ? $"Usuario: {usuario.NombreCompleto} ({usuario.Email})"
            : $"Usuario {id} no encontrado");
    }
}
