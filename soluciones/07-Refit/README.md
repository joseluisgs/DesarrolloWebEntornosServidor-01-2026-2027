# Ejemplo 06: Consumo de APIs con Refit

## Descripción

Este ejemplo muestra cómo consumir una API REST externa (**JSONPlaceholder**) usando **Refit**, una librería que genera implementaciones de interfaces HTTP de forma declarativa.

## Conceptos Clave

### ¿Qué es Refit?

Refit es una librería que transforma interfaces C# en clientes HTTP reales usando atributos. En lugar de escribir `HttpClient.GetAsync(...)` manualmente, Defines una interfaz y Refit genera la implementación:

```csharp
// ❌ MANUALLY: HttpClient tradicional (mucho código repetitivo)
var response = await httpClient.GetAsync($"/users/{id}");
response.EnsureSuccessStatusCode();
var json = await response.Content.ReadAsStringAsync();
var usuario = JsonSerializer.Deserialize<Usuario>(json);

// ✅ CON REFIT: Una línea limpia
var usuario = await api.GetUsuarioByIdAsync(id);
```

📌 Ejemplo real: **Instagram** usa patrones similares para comunicarse con sus microservicios. Cada servicio expone una API y los clientes la consumen mediante interfaces tipadas.

### Refit vs HttpClient Manual

| Característica | HttpClient Manual | Refit |
|----------------|-------------------|-------|
| **Código** | Mucho boilerplate | Declarativo |
| **Tipado** | Strings y serialization manual | Fuertemente tipado |
| **Mantenimiento** | Difícil de refactorizar | Fácil con interfaces |
| **Testing** | Mock complejo | Mock de interfaz simple |
| **Errores** | Try-catch manual | ApiException automática |

### Atributos de Refit

| Atributo | Método HTTP | Ejemplo |
|----------|-------------|---------|
| `[Get("/path")]` | GET | `[Get("/users/{id}")]` |
| `[Post("/path")]` | POST | `[Post("/users")]` |
| `[Put("/path")]` | PUT | `[Put("/users/{id}")]` |
| `[Delete("/path")]` | DELETE | `[Delete("/users/{id}")]` |

### IHttpClientFactory

`IHttpClientFactory` gestiona el ciclo de vida de las instancias `HttpClient`:

- **Resolución de DNS**: Actualiza IPs periódicamente
- **Pool de conexiones**: Reutiliza conexiones TCP
- **Lifetime management**: Evita el problema de socket exhaustion

```csharp
services.AddHttpClient("nombre", client =>
{
    client.BaseAddress = new Uri("https://api.ejemplo.com");
})
.AddRefitClient<IApi>();
```

### Result Object Pattern (ROP)

En lugar de usar excepciones para errores de negocio, usamos `Result<T, E>`:

```csharp
// Resultado exitoso
var result = Result<Usuario, DomainError>.Ok(usuario);

// Resultado con error
var result = Result<Usuario, DomainError>.Fail(
    new DomainError.NotFound("Usuario", id));

// Pattern matching para manejar
if (result is Result<Usuario, DomainError>.Success success)
{
    Console.WriteLine(success.Value.Name);
}
else if (result is Result<Usuario, DomainError>.Failure failure)
{
    Console.WriteLine(failure.Error);
}
```

## Prerrequisitos

1. .NET 10 SDK
2. Conexión a internet (JSONPlaceholder es una API pública)

## Ejecución

```bash
dotnet run
```

No necesita Docker ni base de datos. JSONPlaceholder es una API demo gratuita.

## Estructura

```
06-Refit/
├── 06-Refit.slnx
├── 06-Refit/
│   ├── 06-Refit.csproj
│   ├── Program.cs
│   ├── Api/
│   │   └── IJsonPlaceholderApi.cs
│   ├── Models/
│   │   └── Usuario.cs
│   ├── Dto/
│   │   ├── CreateUserRequest.cs
│   │   └── UpdateUserRequest.cs
│   ├── Services/
│   │   └── UsuarioService.cs
│   └── Errors/
│       └── DomainError.cs
└── README.md
```

## Paquetes NuGet

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `Refit` | 8.0.0 | Cliente HTTP declarativo |
| `Refit.Newtonsoft.Json` | 8.0.0 | Serialización con Newtonsoft |
| `Microsoft.Extensions.Http` | 9.0.0 | IHttpClientFactory |
| `Microsoft.Extensions.DependencyInjection` | 9.0.0 | Inyección de dependencias |

## Casos de Uso

- **Microservicios**: Consumo de APIs internas
- **Integraciones externas**: APIs de terceros (Stripe, Twilio, etc.)
- **BFF (Backend for Frontend)**: Agregar múltiples APIs para el frontend

## Referencias

- [Refit (GitHub)](https://github.com/reactiveui/refit)
- [Refit Documentation](https://refit-httpclient.io/)
- [JSONPlaceholder](https://jsonplaceholder.typicode.com/)
- [IHttpClientFactory (Microsoft)](https://learn.microsoft.com/es-es/dotnet/core/extensions/httpclient-factory)
