- [18. Consumo de APIs: HttpClient, Refit y Polly](#18-consumo-de-apis-httpclient-refit-y-polly)
  - [18.1. HttpClient: La Base de la Comunicación HTTP](#181-httpclient-la-base-de-la-comunicación-http)
  - [18.2. IHttpClientFactory: Gestionar HttpClient Correctamente](#182-ihttpclientfactory-gestionar-httpclient-correctamente)
  - [18.3. Refit: Interfaces Tipadas para APIs](#183-refit-interfaces-tipadas-para-apis)
  - [18.4. Polly: Resiliencia y Reintentos](#184-polly-resiliencia-y-reintentos)
  - [18.5. Ejemplo Completo: API con Refit y Polly](#185-ejemplo-completo-api-con-refit-y-polly)


# 18. Consumo de APIs: HttpClient, Refit y Polly

> 💡 **Punto de partida:** Tu app necesita hablar con una API externa (Stripe para pagos, Spotify para música, una API del gobierno para datos). Pero las APIs fallan: el servidor se cae, la red se corta, hay rate limiting... ¿Cómo gestionas esa comunicación de forma robusta? Con `HttpClient`, `Refit` para código limpio y `Polly` para sobrevivir a los fallos.

En este tema aprenderás a consumir APIs con `HttpClient`, `IHttpClientFactory`, `Refit` (interfaces tipadas) y `Polly` (retry, circuit breaker).

**Objetivos de aprendizaje:**

- Usar `HttpClient` para peticiones HTTP básicas
- Entender por qué `IHttpClientFactory` es obligatorio en ASP.NET Core
- Crear APIs tipadas con Refit
- Aplicar resiliencia con Polly: retry y circuit breaker
- Construir un cliente de API completo y robusto

## 18.1. HttpClient: La Base de la Comunicación HTTP

`HttpClient` es la clase de .NET para hacer peticiones HTTP. Soporta GET, POST, PUT, DELETE, etc.

### Uso básico

```csharp
using var client = new HttpClient();

// GET: obtener datos
string json = await client.GetStringAsync("https://api.ejemplo.com/usuarios");
var usuarios = JsonSerializer.Deserialize<List<Usuario>>(json);

// POST: enviar datos
var nuevo = new { Nombre = "Ana", Email = "ana@email.com" };
string contenido = JsonSerializer.Serialize(nuevo);
var httpContent = new StringContent(contenido, Encoding.UTF8, "application/json");

HttpResponseMessage respuesta = await client.PostAsync("https://api.ejemplo.com/usuarios", httpContent);
respuesta.EnsureSuccessStatusCode(); // Lanza excepción si no es 2xx

string respuestaJson = await respuesta.Content.ReadAsStringAsync();
```

### Métodos de HttpClient

| Método | HTTP | Ejemplo |
|--------|------|---------|
| `GetAsync` | GET | `client.GetAsync(url)` |
| `PostAsync` | POST | `client.PostAsync(url, content)` |
| `PutAsync` | PUT | `client.PutAsync(url, content)` |
| `DeleteAsync` | DELETE | `client.DeleteAsync(url)` |
| `GetStringAsync` | GET | `client.GetStringAsync(url)` |
| `GetFromJsonAsync<T>` | GET | `client.GetFromJsonAsync<T>(url)` |
| `PostAsJsonAsync<T>` | POST | `client.PostAsJsonAsync(url, obj)` |

### Los problemas de HttpClient

```csharp
// ❌ MALO: Crear HttpClient cada vez
public async Task<Usuario> ObtenerUsuarioAsync(int id)
{
    using var client = new HttpClient(); // ¡Cada petición crea y destruye un socket!
    return await client.GetFromJsonAsync<Usuario>($"https://api.com/usuarios/{id}");
}

// ❌ MALO: HttpClient estático (Socket Exhaustion)
private static readonly HttpClient _client = new();
// Si haces miles de peticiones, los sockets se agotan
```

> ⚠️ **Advertencia:** `HttpClient` **no debe** crearse ni destruirse frecuentemente. Cada instancia consume un socket del sistema operativo. Si creas miles de instancias, el sistema operativo se queda sin sockets y las peticiones fallan con `SocketException`.

## 18.2. IHttpClientFactory: Gestionar HttpClient Correctamente

`IHttpClientFactory` es la solución de .NET a los problemas de `HttpClient`. Gestiona un pool de clientes HTTP y renueva las conexiones automáticamente.

### Configuración

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registrar IHttpClientFactory
builder.Services.AddHttpClient();

// Registrar un cliente con nombre (con configuración)
builder.Services.AddHttpClient("spotify", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar un cliente tipado
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
});

var app = builder.Build();
```

### Uso con nombre

```csharp
public class MusicService(IHttpClientFactory httpClientFactory)
{
    public async Task<Playlist> ObtenerPlaylistAsync(string playlistId)
    {
        var client = httpClientFactory.CreateClient("spotify");
        return await client.GetFromJsonAsync<Playlist>($"playlists/{playlistId}");
    }
}
```

### Uso con cliente tipado

```csharp
// Interfaz
public interface ISpotifyService
{
    Task<Playlist?> ObtenerPlaylistAsync(string id);
    Task<SearchResult> BuscarAsync(string query);
}

// Implementación
public class SpotifyService(HttpClient client) : ISpotifyService
{
    public async Task<Playlist?> ObtenerPlaylistAsync(string id)
    {
        return await client.GetFromJsonAsync<Playlist>($"playlists/{id}");
    }

    public async Task<SearchResult> BuscarAsync(string query)
    {
        return await client.GetFromJsonAsync<SearchResult>($"search?q={query}&type=track");
    }
}

// Registro
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
});
```

> 💡 **Consejo:** Usa **cliente tipado** (`AddHttpClient<IService, Service>()`) cuando el servicio consume una API específica. Usa **cliente con nombre** (`AddHttpClient("nombre")`) cuando necesitas múltiples clientes con configuraciones diferentes.

📌 **Ejemplo real:** En una app de e-commerce, `IPedidoService` usa un cliente tipado para la API de Stripe (pagos), otro para la API de Correos (envíos) y otro para la API de NIF/CIF (validación fiscal). Cada uno tiene su URL, timeout y cabeceras.

## 18.3. Refit: Interfaces Tipadas para APIs

**Refit** convierte una interfaz C# en un cliente HTTP. Es como la Inversión de Dependencias pero para APIs: defines **qué** necesitas (la interfaz) y Refit se encarga del **cómo** (las peticiones HTTP).

### Instalación

```bash
dotnet add package Refit
dotnet add package Refit.HttpClientFactory
```

### Definir la interfaz

```csharp
using Refit;

// Definir la API como una interfaz
public interface IUsuarioApi
{
    [Get("/usuarios")]
    Task<List<Usuario>> ObtenerTodosAsync();

    [Get("/usuarios/{id}")]
    Task<Usuario?> ObtenerPorIdAsync(int id);

    [Post("/usuarios")]
    Task<Usuario> CrearAsync([Body] Usuario usuario);

    [Put("/usuarios/{id}")]
    Task<Usuario> ActualizarAsync(int id, [Body] Usuario usuario);

    [Delete("/usuarios/{id}")]
    Task EliminarAsync(int id);

    [Get("/usuarios/buscar")]
    Task<List<Usuario>> BuscarAsync([Query] string nombre);
}
```

### Configurar Refit

```csharp
// Program.cs
builder.Services
    .AddRefitClient<IUsuarioApi>(settings => new RefitSettings
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
    })
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.ejemplo.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
```

### Usar la interfaz

```csharp
public class UsuarioService(IUsuarioApi api) // Refit crea la implementación automáticamente
{
    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await api.ObtenerTodosAsync(); // ¡Una línea! Refit hace la petición HTTP
    }

    public async Task<Usuario?> CrearAsync(string nombre, string email)
    {
        var usuario = new Usuario { Nombre = nombre, Email = email };
        return await api.CrearAsync(usuario);
    }
}
```

### Comparativa: HttpClient vs Refit

| Aspecto | HttpClient manual | Refit |
|---------|-------------------|-------|
| **Código** | Verboso (serializar, URLs, etc.) | Conciso (solo interfaz) |
| **Mantenimiento** | Cambiar URL = buscar en todo el código | Cambiar atributo = un solo sitio |
| **Testeable** | Difícil (necesitas mockear HttpClient) | Fácil (mockear la interfaz) |
| **Type-safe** | ❌ Strings para URLs y Bodies | ✅ Tipado en compile-time |

> 💡 **Consejo:** Si tu app consume una API externa, **siempre** usa Refit. El ahorro de código es enorme y la maintenance es mucho más sencilla.

📌 **Ejemplo real:** En un proyecto real, Refit reduce ~50 líneas de código HTTP por endpoint a ~3 (el atributo + la firma del método). Si la API tiene 30 endpoints, ahorras ~1500 líneas de código repetitivo.

## 18.4. Polly: Resiliencia y Reintentos

**Polly** es una librería que añade resiliencia a las llamadas HTTP: reintentos automáticos, circuit breakers, timeouts y rate limiting.

### Instalación

```bash
dotnet add package Microsoft.Extensions.Http.Polly
```

### Retry: Reintentos automáticos

```csharp
// Configurar reintentos con Polly
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()           // Si falla la petición
        .OrResult<HttpResponseMessage>(r =>       // O si el status code es 5xx
            (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: 3,                        // 3 reintentos
            sleepDurationProvider: attempt =>
                TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Backoff exponencial: 2s, 4s, 8s
            onRetry: (outcome, delay, attempt, context) =>
            {
                Console.WriteLine($"Reintento {attempt}: esperando {delay.TotalSeconds}s");
            }
        ));
```

### Circuit Breaker: Cortocircuito

El Circuit Breaker abre el circuito cuando hay demasiados fallos, evitando seguir llamando a un servicio caído.

```csharp
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5, // Después de 5 fallos
            durationOfBreak: TimeSpan.FromSeconds(30), // Abrir circuito 30s
            onBreak: (result, duration) =>
            {
                Console.WriteLine($"Circuito ABIERTO durante {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuito CERRADO, reanudando llamadas");
            }
        ));
```

### Combinar Retry + Circuit Breaker

```csharp
// Política compuesta: retry + circuit breaker
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

builder.Services.AddHttpClient<ISpotifyService, SpotifyService>()
    .AddPolicyHandler(policyWrap);
```

### Timeout

```csharp
// Timeout con Polly (más control que HttpClient.Timeout)
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>()
    .AddPolicyHandler(Policy
        .TimeoutAsync(TimeSpan.FromSeconds(10)));
```

| Política | Descripción | Cuándo usar |
|----------|-------------|-------------|
| **Retry** | Reintentar N veces con backoff | Errores temporales (red, timeout) |
| **Circuit Breaker** | Parar llamadas si hay muchos fallos | Servicio caído (evitar efecto cascada) |
| **Timeout** | Lanzar error si tarda demasiado | APIs lentas |
| **Bulkhead** | Limitar llamadas concurrentes | Proteger recursos limitados |

> 📝 **Nota:** El patrón **Circuit Breaker** es como un fusible eléctrico: si hay demasiados cortocircuitos, se abre y corta la corriente para evitar un incendio. Después de un tiempo, vuelve a intentarlo.

📌 **Ejemplo real:** Netflix usa Circuit Breaker en todas sus llamadas a microservicios. Si el servicio de recomendaciones se cae, en vez de seguir enviando peticiones que van a fallar (y congestionar aún más el servicio caído), abre el circuito y devuelve recomendaciones por defecto. Cuando el servicio se recupera, el circuito se cierra automáticamente.

## 18.5. Ejemplo Completo: API con Refit y Polly

### Interfaz de la API

```csharp
using Refit;

public interface IPokemonApi
{
    [Get("/api/v2/pokemon/{id}")]
    Task<Pokemon?> ObtenerPorIdAsync(int id);

    [Get("/api/v2/pokemon")]
    Task<PokemonList> ObtenerTodosAsync([Query] int limit = 100);
}
```

### Modelo de datos

```csharp
public record Pokemon(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Nombre,
    [property: JsonPropertyName("height")] int Altura,
    [property: JsonPropertyName("weight")] int Peso,
    [property: JsonPropertyName("sprites")] Sprites Sprites
);

public record Sprites(
    [property: JsonPropertyName("front_default")] string? Imagen
);

public record PokemonList(
    [property: JsonPropertyName("count")] int Total,
    [property: JsonPropertyName("results")] List<PokemonRef> Resultados
);

public record PokemonRef(
    [property: JsonPropertyName("name")] string Nombre,
    [property: JsonPropertyName("url")] string Url
);
```

### Configuración completa

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registrar Refit + Polly juntos
builder.Services
    .AddRefitClient<IPokemonApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://pokeapi.co/");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

var app = builder.Build();

app.MapGet("/pokemon/{id}", async (int id, IPokemonApi api) =>
{
    var pokemon = await api.ObtenerPorIdAsync(id);
    return pokemon is not null
        ? Results.Ok(pokemon)
        : Results.NotFound();
});

app.Run();
```

> 💡 **Consejo:** Para el examen, recuerda la combinación ideal: **Refit** (para código limpio) + **IHttpClientFactory** (para gestión de sockets) + **Polly** (para resiliencia). Es el patrón estándar en producción.

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **HttpClient** | Clase base para peticiones HTTP |
| **IHttpClientFactory** | Gestiona un pool de clientes (evita socket exhaustion) |
| **Cliente tipado** | HttpClient inyectado directamente en un servicio |
| **Refit** | Convierte interfaces en clientes HTTP con atributos |
| **Polly** | Librería de resiliencia: retry, circuit breaker, timeout |
| **Retry** | Reintentar automáticamente tras un fallo |
| **Circuit Breaker** | Parar llamadas si hay demasiados fallos consecutivos |

En el siguiente punto veremos configuración y logging: `appsettings.json`, `IConfiguration`, `IOptions<T>` y Serilog para logging estructurado.
