# ============================================================
# Dockerfile multi-etapa para Ejemplo 12: Docker Hello World
# ============================================================

# ETAPA 1: Build + Tests
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO archivos de proyecto (NO obj/ ni bin/)
COPY *.slnx nuget.config ./
COPY 12-Docker-HelloWorld/12-Docker-HelloWorld.csproj ./12-Docker-HelloWorld/
COPY 12-Docker-HelloWorld.Test/12-Docker-HelloWorld.Test.csproj ./12-Docker-HelloWorld.Test/

# Restaurar paquetes (descarga limpia, sin caché de Windows)
RUN dotnet restore

# Copiar código fuente
COPY 12-Docker-HelloWorld/ ./12-Docker-HelloWorld/
COPY 12-Docker-HelloWorld.Test/ ./12-Docker-HelloWorld.Test/

# Compilar
RUN dotnet build --configuration Release --no-restore

# EJECUTAR TESTS — si fallan, la imagen NO se crea
RUN dotnet test --configuration Release --no-restore --verbosity normal

# Publicar
RUN dotnet publish 12-Docker-HelloWorld/12-Docker-HelloWorld.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ETAPA 2: Runtime (imagen final ligera + seguro)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Crear usuario no-root por seguridad
RUN useradd -m appuser
USER appuser

# Copiar solo el binario publicado
COPY --from=build /app/publish .

# Puerto de la Minimal API
EXPOSE 8080

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "12-Docker-HelloWorld.dll"]
