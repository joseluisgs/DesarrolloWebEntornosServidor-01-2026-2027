# ============================================================
# Dockerfile multi-etapa — Ejemplo 09: Accidentes Madrid
# Console app: lee CSV, analiza con LINQ/PLINQ/DataFrames
# ============================================================

# ETAPA 1: Build + Tests
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx ./
COPY 09-AccidentesMadrid/09-AccidentesMadrid.csproj ./09-AccidentesMadrid/
COPY 09-AccidentesMadrid.Tests/09-AccidentesMadrid.Tests.csproj ./09-AccidentesMadrid.Tests/
RUN dotnet restore

COPY 09-AccidentesMadrid/ ./09-AccidentesMadrid/
COPY 09-AccidentesMadrid.Tests/ ./09-AccidentesMadrid.Tests/
RUN dotnet build --configuration Release --no-restore

RUN dotnet test --configuration Release --no-restore --verbosity normal

RUN dotnet publish 09-AccidentesMadrid/09-AccidentesMadrid.csproj \
    --configuration Release --no-restore --output /app/publish

# ETAPA 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN useradd -m appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "09-AccidentesMadrid.dll"]
