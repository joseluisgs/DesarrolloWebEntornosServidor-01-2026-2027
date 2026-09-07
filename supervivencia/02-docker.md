# Docker — Comandos de Supervivencia

> Referencia rápida para contenedores, imágenes, Docker Compose y Dockerfile.

## Contenedores

```bash
# Listar contenedores en ejecución
docker ps

# Listar todos (incluyendo parados)
docker ps -a

# Arrancar y ejecutar contenedor desde una imagen
docker run -d --name mi-postgres -p 5432:5432 -e POSTGRES_PASSWORD=secret postgres:16-alpine
#   -d              detach (segundo plano)
#   --name          nombre del contenedor
#   -p host:cont    mapeo de puertos
#   -e              variable de entorno
#   -v vol:/path    montar volumen
#   imagen:tag      qué imagen usar

# Arrancar con volumen y datos persistentes
docker run -d --name mi-redis -p 6379:6379 -v redis_data:/data redis:7-alpine

# Arrancar y entrar en la terminal
docker run -it --rm ubuntu bash
#   -it             interactivo + terminal
#   --rm            eliminar al salir
```

> 💡 **Consejo:** Siempre usa `--name` para identificar tus contenedores. Sin nombre, Docker genera uno aleatorio como `angry_newton` y no lo encuentras después.

> 🔧 **Truco:** `docker run -it --rm` es perfecto para probar algo rápido sin dejar basura. El contenedor se borra al salir.

> ⚠️ **Advertencia:** El flag `-f` en `docker rm -f` fuerza la eliminación de un contenedor en ejecución. Puede causar pérdida de datos si no tienes volumen.

```bash
# Arrancar un contenedor parado
docker start <nombre>

# Parar contenedor
docker stop <nombre>

# Reiniciar
docker restart <nombre>

# Ejecutar comando dentro del contenedor
docker exec -it <nombre> bash        # Linux
docker exec -it <nombre> cmd         # Windows
docker exec -it <nombre> sh          # Alpine (sin bash)

# Ver logs
docker logs <nombre>
docker logs -f <nombre>              # seguir en tiempo real
docker logs --tail 100 <nombre>     # últimas 100 líneas

# Eliminar contenedor
docker rm <nombre>                   # parado
docker rm -f <nombre>               # forzar (en ejecución)
```

> 💡 **Analogía:** Un contenedor es como una máquina virtual ligera. `docker run` la crea y arranca, `docker exec` es como hacer SSH a ella, `docker stop` la apaga.

## Imágenes

```bash
# Listar imágenes
docker images

# Descargar imagen
docker pull postgres:16-alpine

# Eliminar imagen
docker rmi <imagen>

# Eliminar imágenes sin usar
docker image prune -a

# Ver historial de capas
docker history <imagen>
```

## Volúmenes

```bash
# Listar volúmenes
docker volume ls

# Crear volumen
docker volume create mi-volumen

# Eliminar volumen
docker volume rm mi-volumen

# Eliminar todos los volúmenes sin usar
docker volume prune

# Ver detalles
docker volume inspect mi-volumen
```

## Redes

```bash
# Listar redes
docker network ls

# Crear red
docker network create mi-red

# Eliminar red
docker network rm mi-red
```

## Limpieza total

```bash
# Eliminar todo (contenedores parados, imágenes sin usar, volúmenes huérfanos)
docker system prune -a --volumes

# Solo contenedores y redes
docker system prune

# Ver espacio en uso
docker system df
```

> ⚠️ **Advertencia:** `docker system prune -a --volumes` borra TODO: imágenes, volúmenes, redes. No lo uses si tienes contenedores importantes que no están corriendo.

> 🔧 **Truco:** Usa `docker system df` antes de limpiar para ver cuánto espacio ocupan imágenes, contenedores y volúmenes.

## Docker Compose

```bash
# Arrancar servicios (en segundo plano)
docker compose up -d

# Parar servicios
docker compose down

# Parar y eliminar volúmenes
docker compose down -v

# Ver logs
docker compose logs
docker compose logs -f <servicio>

# Ver servicios activos
docker compose ps

# Reconstruir imágenes
docker compose build
docker compose up -d --build

# Ejecutar comando en un servicio
docker compose exec <servicio> bash
docker compose exec <servicio> dotnet run
```

> 💡 **Consejo:** `docker compose down -v` elimina también los volúmenes. Útil para empezar de cero, pero CUIDADO: borra la base de datos persistida.

> 🔧 **Truco:** `docker compose up -d --build` reconstruye las imágenes ANTES de arrancar. Si cambiaste el Dockerfile, siempre usa este.

## Docker Compose — Puerto y volúmenes

```yaml
services:
  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"              # host:contenedor
    volumes:
      - postgres_data:/var/lib/postgresql/data   # volumen con nombre
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql  # montar fichero
    environment:
      POSTGRES_DB: mi_bd
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: secret

volumes:
  postgres_data:
```

## Dockerfile básico

```dockerfile
# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiProyecto.dll"]
```

> 💡 **Consejo:** Multi-stage builds (etapa build + etapa runtime) reducen el tamaño de la imagen de cientos de MB a decenas. El SDK solo se usa para compilar, no para ejecutar.

> ⚠️ **Advertencia:** NUNCA guardes contraseñas o secrets en el Dockerfile. Usa variables de entorno (`-e`) o Docker secrets.

## Dockerfile — instrucciones clave

| Instrucción | Qué hace | Ejemplo |
|-------------|----------|---------|
| `FROM` | Imagen base | `FROM mcr.microsoft.com/dotnet/sdk:10.0` |
| `WORKDIR` | Directorio de trabajo | `WORKDIR /src` |
| `COPY` | Copiar archivos | `COPY . .` |
| `RUN` | Ejecutar comando | `RUN dotnet restore` |
| `EXPOSE` | Documentar puerto | `EXPOSE 8080` |
| `ENTRYPOINT` | Comando de inicio | `ENTRYPOINT ["dotnet", "app.dll"]` |
| `ENV` | Variable de entorno | `ENV ASPNETCORE_ENVIRONMENT=Production` |

## Errores comunes

| Error | Solución |
|-------|----------|
| `port is already allocated` | `docker ps` → ver qué usa el puerto, pararlo |
| `no configuration file provided` | Estás en la carpeta equivocada, busca el `docker-compose.yml` |
| `Cannot connect to the Docker daemon` | Abrir Docker Desktop |
| `image not found` | Revisar nombre y tag de la imagen |
