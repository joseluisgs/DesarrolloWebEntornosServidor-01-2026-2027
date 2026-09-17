# Ejemplo 12: Docker Hello World

Console + Minimal API + Tests + Docker multi-etapa + Docker Compose.

> 💡 **Nota:** Este ejemplo usa Docker, pero funciona igual con Podman. Solo cambia `docker` por `podman` en los comandos. El Dockerfile y docker-compose.yml son idénticos para ambos.

## Estructura

```
12-Docker-HelloWorld/
├── 12-Docker-HelloWorld.slnx       # Solution
├── Dockerfile                      # Imagen multi-etapa
├── docker-compose.yml              # Orquestación
├── 12-Docker-HelloWorld/           # App principal
│   ├── 12-Docker-HelloWorld.csproj
│   └── Program.cs                  # Console + Minimal API
└── 12-Docker-HelloWorld.Test/      # Tests
    ├── 12-Docker-HelloWorld.Test.csproj
    └── HelloWorldTests.cs
```

## Qué hace la app

1. **Consola**: Imprime "Hola desde Docker" al arrancar
2. **Web**: Responde "Hola desde Docker" en `http://localhost:5000`
3. **Health**: Endpoint `/health` con estado y timestamp

## Ejecución sin Docker

```bash
# Compilar y ejecutar
dotnet run --project 12-Docker-HelloWorld

# Abrir navegador
# http://localhost:5000
```

## Ejecución con Docker Compose

```bash
# Build + ejecutar (primera vez)
docker compose up --build
# o con Podman: podman compose up --build

# Ejecutar (ya construido)
docker compose up

# En segundo plano
docker compose up -d

# Parar
docker compose down

# Ver logs
docker compose logs -f
```

## Probar

```bash
# Consola (se ve en los logs de Docker)
docker compose logs

# Web
curl http://localhost:5000
# → "Hola desde Docker (web)"

# Health
curl http://localhost:5000/health
# → {"status":"Healthy","timestamp":"2026-09-08T..."}
```

## Dockerfile: ¿Por qué es así?

### Etapa 1: Build (imagen pesada)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

- **`sdk:10.0`**: Imagen con el SDK completo (compilador, dotnet CLI, nuget). Pesada (~700MB).
- Se usa SOLO para compilar. Luego se descarta.

**¿Por qué copiar los `.csproj` primero?**

```dockerfile
COPY *.slnx nuget.config ./
COPY 12-Docker-HelloWorld/12-Docker-HelloWorld.csproj ./12-Docker-HelloWorld/
COPY 12-Docker-HelloWorld.Test/12-Docker-HelloWorld.Test.csproj ./12-Docker-HelloWorld.Test/
RUN dotnet restore
COPY 12-Docker-HelloWorld/ ./12-Docker-HelloWorld/
COPY 12-Docker-HelloWorld.Test/ ./12-Docker-HelloWorld.Test/
```

Docker usa **caché por capas**. Si solo cambia el código (`.cs`) pero no los `.csproj`, `dotnet restore` no se vuelve a ejecutar. Ahorra minutos en builds sucesivos.

**¿Por qué NO copiar `obj/` y `bin/`?**

```dockerfile
# ❌ MALO: Copia la caché de Windows al contenedor Linux
COPY . .

# ✅ BUENO: Copia solo código fuente, restore limpio en Linux
COPY 12-Docker-HelloWorld/ ./12-Docker-HelloWorld/
COPY 12-Docker-HelloWorld.Test/ ./12-Docker-HelloWorld.Test/
```

Si copias `obj/` y `bin/` de Windows, el contenedor Linux recibe caché de paquetes incompatible. El `restore` parece funcionar pero luego falla el build. Usamos `.dockerignore` para excluir esas carpetas.

**¿Por qué ejecutar tests en el Dockerfile?**

```dockerfile
RUN dotnet test --configuration Release --no-restore
```

Si un test falla, el build de la imagen **se detiene**. Así garantizamos que solo desplegamos código que pasa los tests. Es la regla dorada: **nada se despliega sin tests verdes**.

### Etapa 2: Runtime (imagen ligera + segura)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Crear usuario no-root por seguridad
RUN useradd -m appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "12-Docker-HelloWorld.dll"]
```

**Instrucción por instrucción:**

| Instrucción | Qué hace |
|-------------|----------|
| `FROM aspnet:10.0` | Imagen base con solo el runtime de .NET (~200MB). No tiene SDK. |
| `WORKDIR /app` | Establece `/app` como directorio de trabajo (se crea si no existe). |
| `RUN useradd -m appuser` | Crea un usuario llamado `appuser` con home directory. |
| `USER appuser` | Cambia el usuario de ejecución de root a `appuser`. |
| `COPY --from=build` | Copia el binario publicado de la etapa anterior. |
| `EXPOSE 8080` | **Documenta** que la app usa el puerto 8080 (no abre el puerto, es solo informativo). |
| `ENV ASPNETCORE_URLS=...` | Variable de entorno: indica a ASP.NET Core en qué puerto escuchar. |
| `ENV ASPNETCORE_ENVIRONMENT=...` | Variable de entorno: entorno de ejecución (Development, Production). |
| `ENTRYPOINT` | Comando que se ejecuta al arrancar el contenedor. |

> ⚠️ **Advertencia sobre `EXPOSE`:** `EXPOSE` **NO abre el puerto**. Solo documenta que la app lo usa. El puerto real se abre con `-p` en `docker run` o `ports:` en `docker-compose.yml`.

> 💡 **Consejo:** `ENV` define variables de entorno **dentro del contenedor**. Son como las variables de sistema, pero específicas de la imagen Docker.

### Flujo visual

```mermaid
graph LR
    A["📦 SDK (~700MB)"] -->|build + test + publish| B["📄 Binario"]
    B --> C["📦 Runtime (~200MB)<br/>+ usuario no-root"]
    C --> D["🐳 Imagen final"]

    style A fill:#FF9800,color:#fff
    style C fill:#4CAF50,color:#fff
    style D fill:#2196F3,color:#fff
```

## docker-compose.yml: ¿Por qué es así?

```yaml
services:
  hello-world:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: docker-hello-world
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

**Cláusula por cláusula:**

| Cláusula | Qué hace |
|----------|----------|
| `services:` | Define los contenedores que componen tu app. Puedes tener varios. |
| `hello-world:` | Nombre del servicio (puedes poner el que quieras). |
| `build:` | Indica que Docker debe **construir** la imagen (no descargarla). |
| `context: .` | Carpeta raíz del build. Docker busca el Dockerfile aquí. |
| `dockerfile: Dockerfile` | Nombre del Dockerfile a usar. Si se llama `Dockerfile`, es opcional. |
| `container_name:` | Nombre real del contenedor en Docker (sin esto, Docker genera uno aleatorio). |
| `ports:` | Mapeo de puertos: `host:contenedor`. |
| `"5000:8080"` | El puerto **8080** del contenedor se expone en el **5000** del host. |
| `environment:` | Variables de entorno que se pasan al contenedor. |
| `ASPNETCORE_ENVIRONMENT=Development` | Activa modo Development (logs más verbosos). |

> 💡 **Consejo:** Si cambias el puerto en el Dockerfile (`EXPOSE 8080`), también tienes que cambiar el `ports:` en el docker-compose.yml y la variable `ASPNETCORE_URLS`.

### Comandos de docker-compose

```bash
# Build + arrancar (primera vez)
docker compose up --build
# o con Podman: podman compose up --build

# Arrancar (ya construido)
docker compose up

# Arrancar en segundo plano
docker compose up -d

# Parar y eliminar contenedores
docker compose down

# Ver logs
docker compose logs
docker compose logs -f          # seguir en tiempo real

# Ver contenedores activos
docker compose ps
```

## .dockerignore: ¿Para qué sirve?

```
**/bin/
**/obj/
**/out/
**/.vs/
**/.vscode/
**/*.user
**/*.suo
**/*.log
**/TestResults/
**/publish/
```

Es como el `.gitignore` pero para Docker. Excluye archivos/carpetas que **NO** deben copiarse al contenedor.

**¿Por qué es importante?**

Si no tienes `.dockerignore`, el `COPY . .` copia `obj/` y `bin/` (caché de compilación) al contenedor. En Windows/Linux, esas carpetas tienen formatos diferentes, y el build falla.

> 💡 **Consejo:** Siempre crea un `.dockerignore` en la raíz del proyecto. Es buena práctica, igual que el `.gitignore`.

## Comandos útiles

```bash
# Ver la imagen creada
docker images | grep hello-world
# o con Podman: podman images | grep hello-world

# Ver contenedor en ejecución
docker ps

# Entrar al contenedor
docker exec -it docker-hello-world bash

# Ver logs
docker logs docker-hello-world

# Ver tamaño de la imagen
docker inspect docker-hello-world | grep Size
```
