using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using _07_Refit.Api;
using _07_Refit.Config;
using _07_Refit.Dto;
using _07_Refit.Errors;
using _07_Refit.Models;
using _07_Refit.Services;

// ============================================================
// Ejemplo 06: Consumo de APIs con Refit
// ============================================================
Console.WriteLine("=== Ejemplo 06: Refit + JSONPlaceholder ===\n");

// Configurar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var apiConfig = new ApiConfig();
configuration.GetSection("ApiSettings").Bind(apiConfig);

// Registrar IHttpClientFactory + Refit con configuración desde appsettings.json
var services = new ServiceCollection();
services.AddHttpClient("jsonplaceholder", client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddRefitClient<IJsonPlaceholderApi>();

services.AddSingleton<UsuarioService>();

var provider = services.BuildServiceProvider();
var usuarioService = provider.GetRequiredService<UsuarioService>();

// ============================================================
// GET: Obtener todos los usuarios
// ============================================================
Console.WriteLine("--- GET: Todos los usuarios ---");
var usuarios = await usuarioService.GetAllAsync();
Console.WriteLine($"  Total: {usuarios.Count} usuarios");
foreach (var u in usuarios.Take(3))
{
    Console.WriteLine($"  [{u.Id}] {u.Name} ({u.Username}) - {u.Email}");
}
Console.WriteLine("  ...\n");

// ============================================================
// GET: Obtener usuario por ID
// ============================================================
Console.WriteLine("--- GET: Usuario por ID ---");
var resultado = await usuarioService.GetByIdAsync(1);
if (resultado is Result<Usuario, DomainError>.Success encontrado)
{
    Console.WriteLine($"  Encontrado: {encontrado.Value.Name} - {encontrado.Value.Email}");
}
else if (resultado is Result<Usuario, DomainError>.Failure errorGet)
{
    Console.WriteLine($"  Error: {errorGet.Error}");
}

// ============================================================
// GET: Usuario inexistente
// ============================================================
Console.WriteLine("\n--- GET: Usuario inexistente (ID 9999) ---");
var noExiste = await usuarioService.GetByIdAsync(9999);
if (noExiste is Result<Usuario, DomainError>.Failure errorNoExiste)
{
    Console.WriteLine($"  Error: {errorNoExiste.Error}");
}

// ============================================================
// POST: Crear usuario
// ============================================================
Console.WriteLine("\n--- POST: Crear usuario ---");
var nuevoUsuario = new CreateUserRequest(
    Name: "José Luis García",
    Username: "joseluisgs",
    Email: "jose.luis@iesluisvives.es"
);
var creado = await usuarioService.CreateAsync(nuevoUsuario);
if (creado is Result<Usuario, DomainError>.Success creadoOk)
{
    Console.WriteLine($"  Creado: [{creadoOk.Value.Id}] {creadoOk.Value.Name}");
}

// ============================================================
// POST: Validación de errores
// ============================================================
Console.WriteLine("\n--- POST: Validación (nombre vacío) ---");
var invalido = new CreateUserRequest(Name: "", Username: "test", Email: "test@test.com");
var validacion = await usuarioService.CreateAsync(invalido);
if (validacion is Result<Usuario, DomainError>.Failure errorValidacion)
{
    Console.WriteLine($"  Error de validación: {errorValidacion.Error}");
}

// ============================================================
// PUT: Actualizar usuario
// ============================================================
Console.WriteLine("\n--- PUT: Actualizar usuario ---");
var actualizacion = new UpdateUserRequest(
    Id: 1,
    Name: "José Luis García Sánchez",
    Username: "joseluisgs",
    Email: "jose.luis@iesluisvives.es"
);
var actualizado = await usuarioService.UpdateAsync(1, actualizacion);
if (actualizado is Result<Usuario, DomainError>.Success actualizadoOk)
{
    Console.WriteLine($"  Actualizado: [{actualizadoOk.Value.Id}] {actualizadoOk.Value.Name}");
}

// ============================================================
// DELETE: Eliminar usuario
// ============================================================
Console.WriteLine("\n--- DELETE: Eliminar usuario ---");
var eliminado = await usuarioService.DeleteAsync(1);
if (eliminado is Result<bool, DomainError>.Success)
{
    Console.WriteLine("  Usuario eliminado correctamente");
}

Console.WriteLine("\n=== Fin del ejemplo ===");
