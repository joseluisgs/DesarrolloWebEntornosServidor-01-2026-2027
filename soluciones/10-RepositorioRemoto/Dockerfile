# ============================================================
# Dockerfile multi-etapa — Ejemplo 10: Repositorio Remoto
# Console app: Caché → BD Local → API Remota
# ============================================================

# ETAPA 1: Build + Tests unitarios
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx nuget.config ./
COPY 10-RepositorioRemoto/10-RepositorioRemoto.csproj ./10-RepositorioRemoto/
COPY 10-RepositorioRemoto.Tests/10-RepositorioRemoto.Tests.csproj ./10-RepositorioRemoto.Tests/
RUN dotnet restore

COPY 10-RepositorioRemoto/ ./10-RepositorioRemoto/
COPY 10-RepositorioRemoto.Tests/ ./10-RepositorioRemoto.Tests/
RUN dotnet build --configuration Release --no-restore

# ⚠️ Solo tests unitarios: los de integración (Testcontainers) necesitan el
# daemon de Docker, que NO está disponible dentro de `docker build`.
# Se ejecutan en local con: dotnet test --filter "Category=Integration"
RUN dotnet test --configuration Release --no-restore --verbosity normal \
    --filter "Category!=Integration"

RUN dotnet publish 10-RepositorioRemoto/10-RepositorioRemoto.csproj \
    --configuration Release --no-restore --output /app/publish

# ETAPA 2: Runtime (imagen mínima de .NET, sin ASP.NET: es una consola)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# libgssapi-krb5-2: lo necesita Npgsql para auth Kerberos.
# Sin él, la app arranca igual pero imprime "Cannot load library libgssapi_krb5.so.2".
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

# --chown para que appuser pueda escribir (export a data/)
COPY --from=build --chown=appuser:appuser /app/publish .

# Production = PostgreSQL + Redis (ver docker-compose.yml)
ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "10-RepositorioRemoto.dll"]
