# Práctica 6: Servicio con Almacenamiento Local y Remoto en .NET

- [Práctica 6: Servicio con Almacenamiento Local y Remoto en .NET](#práctica-6-servicio-con-almacenamiento-local-y-remoto-en-net)
  - [Objetivo](#objetivo)
  - [Descripción de la Práctica](#descripción-de-la-práctica)
  - [Tecnologías a Usar](#tecnologías-a-usar)
  - [Docker Compose](#docker-compose)
  - [Pasos de Implementación](#pasos-de-implementación)
  - [Requisitos Funcionales](#requisitos-funcionales)
  - [Consideraciones de Diseño](#consideraciones-de-diseño)
  - [Requisitos Técnicos](#requisitos-técnicos)
  - [Estructura de Proyecto](#estructura-de-proyecto)

---

## Objetivo

Desarrollar un servicio en **ASP.NET Core** que gestione datos con **tres niveles de almacenamiento**: caché (MemoryCache o Redis), base de datos local (EF Core + PostgreSQL) y API REST remota. El servicio realizará operaciones CRUD de forma asíncrona y usará las tecnologías vistas en la UD01.

---

## Descripción de la Práctica

Usaremos la API de **JSONPlaceholder** (https://jsonplaceholder.typicode.com) como almacenamiento remoto. Trabajaremos con el recurso **"users"**:

```json
{
  "id": 1,
  "name": "Leanne Graham",
  "username": "Bret",
  "email": "Sincere@april.biz"
}
```

**Objetivo:** Crear un servicio cacheado de tres niveles: caché → BD local → API REST, con exportación a JSON.

**Solo manejar estos campos:** id, name, username, email.

---

## Tecnologías a Usar

| Tecnología | Para qué | Paquete NuGet |
|------------|----------|---------------|
| **MemoryCache** o **Redis** | Caché en memoria/distribuida | `Microsoft.Extensions.Caching.Memory` o `StackExchange.Redis` + `Microsoft.Extensions.Caching.StackExchangeRedis` |
| **EF Core + PostgreSQL** | BD local | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **Refit** | Cliente HTTP tipado | `Refit` + `Refit.HttpClientFactory` |
| **CSharpFunctionalExtensions** | Manejo de errores con Result | `CSharpFunctionalExtensions` |
| **Serilog** | Logging con rolling files | `Serilog.AspNetCore` + `Serilog.Sinks.Console` + `Serilog.Sinks.File` |
| **System.Reactive** | Notificaciones con Rx.NET | `System.Reactive` |
| **NUnit + Moq + FluentAssertions** | Testing | `NUnit` + `Moq` + `FluentAssertions` |

---

## Docker Compose

Si necesitas instalar PostgreSQL y Redis localmente, usa este `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: academia-postgres
    environment:
      POSTGRES_DB: academia
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: academia-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
  redis_data:
```

### Iniciar servicios

```bash
docker-compose up -d
```

### Verificar que funcionan

```bash
# PostgreSQL
docker exec -it academia-postgres psql -U postgres -c "SELECT 1;"

# Redis
docker exec -it academia-redis redis-cli ping
# Debe devolver: PONG
```

### Parar servicios

```bash
docker-compose down
```

### Conexión desde la app

```json
// appsettings.json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=academia;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

---

## Pasos de Implementación

### 1. Modelo de dominio

```csharp
public record User(
    int Id,
    string Name,
    string Username,
    string Email
);
```

### 2. Entidad de EF Core

```csharp
public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 3. DTOs para la API REST

```csharp
// DTO de respuesta de JSONPlaceholder
public record JsonPlaceholderUserDto(
    int Id,
    string Name,
    string Username,
    string Email
);

// DTO de respuesta del servicio
public record UserResponseDto(
    int Id,
    string Name,
    string Username,
    string Email
);

// DTO de request para crear/actualizar
public record CreateUserRequest(
    string Name,
    string Username,
    string Email
);
```

### 4. Interfaz del servicio

```csharp
public interface IUserService
{
    Task<Result<IEnumerable<UserResponseDto>, DomainError>> GetAllAsync();
    Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id);
    Task<Result<UserResponseDto, DomainError>> CreateAsync(CreateUserRequest request);
    Task<Result<UserResponseDto, DomainError>> UpdateAsync(int id, CreateUserRequest request);
    Task<Result<Unit, DomainError>> DeleteAsync(int id);
    Task<Result<string, DomainError>> ExportToJsonAsync();
}
```

### 5. Configuración de caché

**Opción A: MemoryCache (sin Redis)**

```csharp
// Program.cs
builder.Services.AddMemoryCache();
```

**Opción B: Redis**

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "users_";
});
```

### 6. Interfaz de caché (abstracta)

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}
```

### 7. Implementación con MemoryCache

```csharp
using Microsoft.Extensions.Caching.Memory;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key)
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
        };
        cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }
}
```

### 8. Implementación con Redis

```csharp
using StackExchange.Redis;
using System.Text.Json;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
```

### 9. Cliente HTTP con Refit

```csharp
public interface IJsonPlaceholderApi
{
    [Get("/users")]
    Task<List<JsonPlaceholderUserDto>> GetUsersAsync();

    [Get("/users/{id}")]
    Task<JsonPlaceholderUserDto> GetUserAsync(int id);

    [Post("/users")]
    Task<JsonPlaceholderUserDto> CreateUserAsync([Body] CreateUserRequest user);

    [Put("/users/{id}")]
    Task<JsonPlaceholderUserDto> UpdateUserAsync(int id, [Body] CreateUserRequest user);

    [Delete("/users/{id}")]
    Task DeleteUserAsync(int id);
}
```

### 10. Servicio completo con 3 niveles

```csharp
public class UserService(
    IJsonPlaceholderApi api,
    AppDbContext context,
    ICacheService cache,
    ILogger<UserService> logger
) : IUserService
{
    public async Task<Result<IEnumerable<UserResponseDto>, DomainError>> GetAllAsync()
    {
        return Result.Success<IEnumerable<UserResponseDto>, DomainError>(
            (await context.Users.ToListAsync())
            .Select(u => new UserResponseDto(u.Id, u.Name, u.Username, u.Email)));
    }

    public async Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id)
    {
        // 1. Buscar en caché
        var cached = await cache.GetAsync<UserResponseDto>($"user:{id}");
        if (cached is not null)
            return Result.Success<UserResponseDto, DomainError>(cached);

        // 2. Buscar en BD local
        var entity = await context.Users.FindAsync(id);
        if (entity is not null)
        {
            var dto = new UserResponseDto(entity.Id, entity.Name, entity.Username, entity.Email);
            await cache.SetAsync($"user:{id}", dto);
            return Result.Success<UserResponseDto, DomainError>(dto);
        }

        // 3. Buscar en API REST
        try
        {
            var remote = await api.GetUserAsync(id);
            var user = new UserEntity
            {
                Id = remote.Id, Name = remote.Name,
                Username = remote.Username, Email = remote.Email,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var result = new UserResponseDto(user.Id, user.Name, user.Username, user.Email);
            await cache.SetAsync($"user:{id}", result);
            return Result.Success<UserResponseDto, DomainError>(result);
        }
        catch
        {
            return Result.Failure<UserResponseDto, DomainError>(
                new DomainError.NotFound(id));
        }
    }

    // ... Create, Update, Delete, ExportToJson siguiendo el mismo patrón
}
```

---

## Requisitos Funcionales

- ✅ **Caché en memoria** (`IMemoryCache`) o **Redis** (`StackExchange.Redis`) para lecturas rápidas
- ✅ **BD local** con EF Core + PostgreSQL
- ✅ **API REST** con Refit contra JSONPlaceholder
- ✅ **Exportación a JSON** con `System.Text.Json`
- ✅ **Manejo de errores** con `Result<T, DomainError>` (CSharpFunctionalExtensions)
- ✅ **Logging** con Serilog (console + file con rolling de 24h, retención 7 días)
- ✅ **Notificaciones** con `IObservable<T>` (System.Reactive) o eventos C#
- ✅ **Configuración** externa via `appsettings.json` + `IOptions<T>`

---

## Consideraciones de Diseño

- **Al arrancar:** borrar BD local y cargar desde API REST
- **Cada 30 segundos:** sincronizar BD local con API REST (BackgroundService)
- **GET /api/users:** devolver desde BD local (si está vacía, cargar de API)
- **GET /api/users/{id}:** caché → BD local → API REST
- **POST /api/users:** enviar a API → guardar en BD → caché → devolver 201
- **PUT /api/users/{id}:** enviar a API → actualizar BD y caché → devolver 200
- **DELETE /api/users/{id}:** eliminar de API → BD → caché → devolver 204
- **GET /api/users/export:** serializar a JSON y guardar en fichero

---

## Requisitos Técnicos

### Arquitectura:
- ✅ Todo asíncrono (`async`/`Task<T>`)
- ✅ Modelo de dominio, DTOs, entidades EF Core
- ✅ Mapeos entre DTOs, entidades y dominio
- ✅ Primary constructors en C# 14

### Testing:
- ✅ Tests unitarios con NUnit + Moq + FluentAssertions
- ✅ Patrón AAA (Arrange-Act-Assert)
- ✅ Test con `UseInMemoryDatabase` o TestContainers

### Documentación:
- ✅ Código documentado con XMLDoc
- ✅ README.md con arquitectura, ejecución y configuración
- ✅ Git con commits en español

### Docker (opcional):
- ✅ Dockerfile multi-stage
- ✅ docker-compose.yml con PostgreSQL + Redis

---

## Estructura de Proyecto

```
MiServicio/
├── Program.cs
├── appsettings.json
├── Models/
│   └── User.cs
├── Entity/
│   └── UserEntity.cs
├── Dto/
│   ├── JsonPlaceholderUserDto.cs
│   ├── UserResponseDto.cs
│   └── CreateUserRequest.cs
├── Repositories/
│   └── UserRepository.cs
├── Services/
│   └── UserService.cs
├── Cache/
│   ├── ICacheService.cs
│   ├── MemoryCacheService.cs
│   └── RedisCacheService.cs
├── Api/
│   └── IJsonPlaceholderApi.cs
├── Config/
│   └── AppConfig.cs
├── Infrastructure/
│   └── DependenciesProvider.cs
├── Errors/
│   └── DomainError.cs
├── Tests/
│   └── UserServiceTests.cs
├── Dockerfile
├── docker-compose.yml
└── MiServicio.csproj
```
