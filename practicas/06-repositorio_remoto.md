# Práctica 6: Servicio con Almacenamiento Local y Remoto en .NET

- [Práctica 6: Servicio con Almacenamiento Local y Remoto en .NET](#práctica-6-servicio-con-almacenamiento-local-y-remoto-en-net)
  - [Objetivo](#objetivo)
  - [Descripción](#descripción)
  - [Tecnologías](#tecnologías)
  - [Docker Compose](#docker-compose)
  - [Comportamiento del Sistema](#comportamiento-del-sistema)
  - [Requisitos Técnicos](#requisitos-técnicos)
  - [Estructura de Proyecto](#estructura-de-proyecto)

---

## Objetivo

Desarrollar un servicio en **ASP.NET Core** que gestione datos con **tres niveles de almacenamiento**: caché (MemoryCache o Redis), base de datos local (EF Core + PostgreSQL) y API REST remota. El servicio realizará operaciones CRUD de forma asíncrona.

---

## Descripción

Usaremos la API de **JSONPlaceholder** (https://jsonplaceholder.typicode.com) como almacenamiento remoto. Trabajaremos con el recurso **"users"** con campos: **id**, **name**, **username**, **email**.

**Comportamiento:**
- Al arrancar: borrar BD local y cargar desde API REST
- Cada 60 segundos: sincronizar BD local con API REST (BackgroundService)
- Obtener usuarios: BD local → si vacía, API REST
- Obtener por ID: caché → BD local → API REST
- Crear/Actualizar/Eliminar: API REST → BD local → caché
- Exportar a JSON: serializar usuarios a fichero

---

## Tecnologías

| Tecnología | Para qué | Paquete NuGet |
|------------|----------|---------------|
| **MemoryCache** o **Redis** | Caché | `Microsoft.Extensions.Caching.Memory` o `StackExchange.Redis` |
| **EF Core + PostgreSQL** | BD local | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **Refit** | Cliente HTTP tipado | `Refit` + `Refit.HttpClientFactory` |
| **CSharpFunctionalExtensions** | Manejo de errores | `CSharpFunctionalExtensions` |
| **Serilog** | Logging con rolling files | `Serilog.AspNetCore` + `Serilog.Sinks.Console` + `Serilog.Sinks.File` |
| **System.Reactive** | Notificaciones | `System.Reactive` |
| **NUnit + Moq + FluentAssertions** | Tests | `NUnit` + `Moq` + `FluentAssertions` |

---

## Docker Compose

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

  redis:
    image: redis:7-alpine
    container_name: academia-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  postgres_data:
  redis_data:
```

```bash
# Iniciar
docker-compose up -d

# Verificar PostgreSQL
docker exec -it academia-postgres psql -U postgres -c "SELECT 1;"

# Verificar Redis
docker exec -it academia-redis redis-cli ping  # → PONG

# Parar
docker-compose down
```

### Conexión desde la app

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=academia;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

---

## Comportamiento del Sistema

- **Al arrancar:** borrar BD local y cargar desde API REST
- **Cada 60 segundos:** BackgroundService sincroniza BD local con API REST
- **GET /api/users:** devolver desde BD local (si vacía, cargar de API)
- **GET /api/users/{id}:** caché → BD local → API REST → 404 si no existe
- **POST /api/users:** API REST → BD local → caché → 201 Created
- **PUT /api/users/{id}:** API REST → BD local → caché → 200 OK
- **DELETE /api/users/{id}:** API REST → BD local → caché → 204 No Content
- **GET /api/users/export:** serializar a JSON y guardar en fichero
- **Notificaciones:** IObservable<T> o eventos C# para crear/actualizar/eliminar

---

## Requisitos Técnicos

- ✅ Todo asíncrono (`async`/`Task<T>`)
- ✅ Primary constructors en C# 14
- ✅ Modelo de dominio, DTOs, entidades EF Core
- ✅ Mapeos entre DTOs, entidades y dominio
- ✅ Result<T, DomainError> con CSharpFunctionalExtensions
- ✅ Logging con Serilog (console Warning+, file Information+ con rolling diario)
- ✅ Configuración externa via `appsettings.json` + `IOptions<T>`
- ✅ Tests unitarios con NUnit + Moq + FluentAssertions (patrón AAA)
- ✅ Código documentado con XMLDoc
- ✅ Git con commits en español

### Opcional (se valorará)

- ✅ Docker (Dockerfile multi-stage + docker-compose.yml)
- ✅ Tests de integración con TestContainers
- ✅ Pipeline CI/CD

---

## Estructura de Proyecto

```
MiServicio/
├── Program.cs
├── appsettings.json
├── Models/
├── Entity/
├── Dto/
├── Repositories/
├── Services/
├── Cache/
├── Api/
├── Config/
├── Infrastructure/
├── Errors/
├── Tests/
├── Dockerfile
├── docker-compose.yml
└── MiServicio.csproj
```
