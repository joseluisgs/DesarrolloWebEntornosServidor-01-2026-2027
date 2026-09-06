- [19. Configuración y Logging en .NET](#19-configuración-y-logging-en-net)
  - [19.1. appsettings.json: La Fuente de Verdad](#191-appsettingsjson-la-fuente-de-verdad)
  - [19.2. IConfiguration: Acceder a la Configuración](#192-iconfiguration-acceder-a-la-configuración)
  - [19.3. IOptions\<T\>: Configuración Tipada](#193-ioptionst-configuración-tipada)
  - [19.4. Serilog: Logging Estructurado](#194-serilog-logging-estructurado)
  - [19.5. ILogger en Apps de Consola](#195-ilogger-en-apps-de-consola)
  - [19.6. Logging con Dependencias y Enrichers](#196-logging-con-dependencias-y-enrichers)


# 19. Configuración y Logging en .NET

> 💡 **Punto de partida:** Tu app tiene datos hardcodeados: `"Server=localhost"`, `"ApiKey=12345"`. Cuando pasas de desarrollo a producción, ¿qué haces? ¿Cambiar el código? ¿Crear otra versión? ¡No! La configuración debe estar **fuera del código**: en `appsettings.json`, variables de entorno o la nube. Y para saber qué pasa dentro de tu app en producción, necesitas **logging** estructurado con Serilog.

En este tema aprenderás a gestionar configuración (`appsettings.json`, `IConfiguration`, `IOptions<T>`) y logging (`Serilog`, `ILogger`) en aplicaciones .NET.

**Objetivos de aprendizaje:**

- Usar `appsettings.json` para configuración por entorno
- Acceder a la configuración con `IConfiguration`
- Crear configuraciones tipadas con `IOptions<T>`
- Configurar Serilog para logging estructurado
- Usar `ILogger` en aplicaciones de consola
- Aplicar enrichers para añadir contexto a los logs

## 19.1. appsettings.json: La Fuente de Verdad

`appsettings.json` es el fichero de configuración principal en ASP.NET Core. Puede tener variantes por entorno:

| Fichero | Propósito |
|---------|-----------|
| `appsettings.json` | Configuración base |
| `appsettings.Development.json` | Configuración para desarrollo (override) |
| `appsettings.Production.json` | Configuración para producción (override) |

### Estructura típica

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiApp;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "MiClaveSecretaSuperSegura123!",
    "Issuer": "https://miapp.com",
    "Audience": "https://miapp.com",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://api.ejemplo.com",
    "ApiKey": "mi-api-key-secreta",
    "TimeoutSeconds": 30
  }
}
```

### Variables de entorno y User Secrets

```bash
# Variables de entorno (producción)
export ConnectionStrings__DefaultConnection="Server=prod.db;Database=MiApp;..."

# User Secrets (desarrollo, solo en tu máquina)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..."
```

> 📝 **Nota:** Las variables de entorno sobreescriben `appsettings.json`. El formato usa `__` (doble guion bajo) para anidar: `ConnectionStrings__DefaultConnection`.

> ⚠️ **Advertencia:** **NUNCA** guardes secretos en `appsettings.json` si el fichero se sube a git. Usa User Secrets (desarrollo) o variables de entorno (producción).

## 19.2. IConfiguration: Acceder a la Configuración

`IConfiguration` es la interfaz que da acceso a toda la configuración de la app.

### Uso básico

```csharp
// En un servicio inyectado
public class MiServicio(IConfiguration config)
{
    public void MostrarConfiguracion()
    {
        // Acceso directo por clave
        string? cadenaConexion = config["ConnectionStrings:DefaultConnection"];

        // Secciones anidadas
        string? jwtSecret = config["JwtSettings:SecretKey"];
        int? expiration = config.GetValue<int>("JwtSettings:ExpirationMinutes");

        // Sección completa
        var jwtSection = config.GetSection("JwtSettings");
        string? issuer = jwtSection["Issuer"];
    }
}
```

### IConfigurationRoot:了解 la fuente

```csharp
// Ver de dónde viene cada configuración
IConfigurationRoot config = (IConfigurationRoot)configuration;

foreach (var provider in config.Providers)
{
    Console.WriteLine($"Proveedor: {provider.GetType().Name}");
}

// Salida proveedores típicos:
// EnvironmentVariablesConfigurationProvider
// JsonConfigurationProvider (appsettings.json)
// JsonConfigurationProvider (appsettings.Development.json)
```

> 💡 **Consejo:** El orden de los proveedores importa. Los últimos sobreescriben los primeros. Por defecto: `appsettings.json` → `appsettings.{Environment}.json` → Variables de entorno → User Secrets.

## 19.3. IOptions\<T\>: Configuración Tipada

`IOptions<T>` convierte una sección de `appsettings.json` en un objeto C# tipado. Así evitas los strings mágicos.

### Definir el modelo

```csharp
// Modelo de configuración
public class JwtSettings
{
    public const string SectionName = "JwtSettings"; // Nombre de la sección en JSON

    public string SecretKey { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int ExpirationMinutes { get; init; } = 60;
}

public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; init; } = "";
    public string ApiKey { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 30;
}
```

### Registrar opciones

```csharp
// Program.cs
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection(ApiSettings.SectionName));
```

### Usar opciones inyectadas

```csharp
public class TokenService(IOptions<JwtSettings> jwtOptions)
{
    public string GenerarToken(int userId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### IOptionsSnapshot para actualizaciones en runtime

```csharp
// IOptionsSnapshot: recarga la configuración cuando cambia el fichero
public class MiServicio(IOptionsSnapshot<ApiSettings> options)
{
    public void HacerAlgo()
    {
        var settings = options.Value; // Siempre la versión más reciente
    }
}
```

| Tipo | Recarga | Ciclo de vida |
|------|---------|---------------|
| `IOptions<T>` | ❌ Solo una vez al inicio | Singleton |
| `IOptionsSnapshot<T>` | ✅ Cada petición HTTP | Scoped |
| `IOptionsMonitor<T>` | ✅ En tiempo real | Singleton |

📌 **Ejemplo real:** En una app de e-commerce, la URL de la API de pagos cambia entre desarrollo y producción. Con `IOptions<PaymentSettings>`, el servicio de pagos no necesita saber en qué entorno está: la configuración se carga automáticamente del `appsettings` correcto.

## 19.4. Serilog: Logging Estructurado

**Serilog** es la librería de logging estándar en .NET. Es más potente que el logging por defecto porque soporta **logging estructurado**: los logs son datos queryeables, no solo texto plano.

### Instalación

```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### Configuración básica

```csharp
// Program.cs
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog(); // Reemplazar el logger por defecto

var app = builder.Build();
```

### Configuración en appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithEnvironmentName" ]
  }
}
```

### Uso de ILogger

```csharp
public class PedidoService(ILogger<PedidoService> logger) : IPedidoService
{
    public async Task<Pedido> CrearPedidoAsync(CrearPedidoDto dto)
    {
        logger.LogInformation("Creando pedido para cliente {ClienteId}", dto.ClienteId);

        try
        {
            var pedido = new Pedido(dto);
            await _repository.GuardarAsync(pedido);

            logger.LogInformation("Pedido {PedidoId} creado correctamente", pedido.Id);
            return pedido;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear pedido para cliente {ClienteId}", dto.ClienteId);
            throw;
        }
    }
}
```

### Niveles de log

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| `Trace` | Detalles muy finos (solo debug) | Valor de variables internas |
| `Debug` | Información para desarrolladores | Paso a paso de un algoritmo |
| `Information` | Flujo normal de la app | "Pedido creado", "Usuario logueado" |
| `Warning` | Algo inesperado pero manejable | "API externa lenta", "Cache vacía" |
| `Error` | Error que necesita atención | "Excepción al guardar", "Fallo de BD" |
| `Fatal` | Error catastrófico, la app se cierra | "No se puede conectar al disco" |

> 💡 **Consejo:** Serilog es **estructurado**: puedes hacer queries sobre los logs. En vez de buscar texto plano, puedes buscar `WHERE PedidoId = 42` o `WHERE Nivel = 'Error'` en un Elasticsearch o Seq.

## 19.5. ILogger en Apps de Consola

En apps de consola no hay `Program.cs` de ASP.NET Core, pero podemos configurar Serilog igualmente:

```csharp
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configurar Serilog para consola
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Crear servicios con logging
var services = new ServiceCollection();
services.AddLogging(builder => builder
    .AddSerilog(dispose: true));
services.AddTransient<MiServicio>();

var provider = services.BuildServiceProvider();
var servicio = provider.GetRequiredService<MiServicio>();

servicio.HacerAlgo();

Log.CloseAndFlush(); // Importante: vaciar los logs pendientes
```

### ILogger con Serilog

```csharp
public class MiServicio(ILogger<MiServicio> logger)
{
    public void HacerAlgo()
    {
        logger.LogInformation("Iniciando proceso");

        // Logging estructurado: el objeto se serializa a JSON
        var pedido = new { Id = 42, Cliente = "Ana", Total = 99.99 };
        logger.LogInformation("Procesando pedido {@Pedido}", pedido);

        // Salida en consola:
        // [10:30:00 INF] Procesando pedido {"Id": 42, "Cliente": "Ana", "Total": 99.99}
    }
}
```

> 📝 **Nota:** Cuando usas `{@Pedido}` (con `@`), Serilog serializa el objeto a JSON en el log. Sin `@`, solo muestra `.ToString()`. Siempre usa `@` para objetos complejos.

## 19.6. Logging con Dependencias y Enrichers

### Enrichers: Añadir contexto a los logs

Los enrichers añaden automáticamente información a cada log: nombre de máquina, versión de la app, ID de petición, etc.

```csharp
// Instalar enrichers
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
dotnet add package Serilog.AspNetCore.RequestLogging

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()          // Nombre del servidor
    .Enrich.WithThreadId()             // ID del hilo
    .Enrich.WithEnvironmentName()       // "Development", "Production"
    .Enrich.WithAssemblyName()          // Nombre del ensamblado
    .Enrich.WithProperty("AppVersion", "1.0.0") // Propiedad custom
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}")
    .CreateLogger();
```

### Request Logging para ASP.NET Core

```csharp
// Log automático de cada petición HTTP
var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// Logs automáticos como:
// [10:30:00 INF] HTTP GET /api/personas respondió 200 en 45.231ms
```

📌 **Ejemplo real:** En producción, Seq o Elasticsearch reciben los logs de Serilog. Puedes hacer queries como `SELECT * FROM Logs WHERE Nivel = 'Error' AND Propiedades.ClienteId = 42` para encontrar todos los errores de un cliente específico. Con logs de texto plano, tendrías que usar `grep` y rezar.

> ⚠️ **Advertencia:** **NUNCA** logues datos sensibles: contraseñas, números de tarjeta de crédito, DNI. Los logs pueden ser accedidos por personas no autorizadas. Si necesitas logear parte de un dato sensible, enmascara: `"****-****-****-" + ultimosCuatro`.

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **appsettings.json** | Fichero de configuración principal |
| **appsettings.{Environment}.json** | Configuración por entorno (override) |
| **Variables de entorno** | Sobreescriben appsettings.json (con `__`) |
| **IConfiguration** | Interfaz para acceder a la configuración |
| **IOptions\<T\>** | Configuración tipada (Singleton) |
| **IOptionsSnapshot\<T\>** | Configuración recargable por petición (Scoped) |
| **Serilog** | Librería de logging estructurado |
| **ILogger** | Interfaz para escribir logs |
| **Enrichers** | Añaden contexto automático a los logs |
| **Request Logging** | Log automático de cada petición HTTP |

En el siguiente punto veremos Entity Framework Core: el ORM de .NET para trabajar con bases de datos de forma elegante y tipada.
