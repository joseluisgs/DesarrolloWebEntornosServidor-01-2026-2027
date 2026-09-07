# .NET CLI — Comandos de Supervivencia

> Referencia rápida de `dotnet` para desarrollo en C#/.NET.

## Crear proyectos

```bash
# Crear proyecto de consola
dotnet new console -n MiProyecto
dotnet new console -n MiProyecto -f net10.0

# Crear proyecto de类库
dotnet new classlib -n MiLibreria

# Crear proyecto de test (NUnit)
dotnet new nunit -n MiProyecto.Test

# Crear solution (.slnx en .NET 10+)
dotnet new sln -n MiSolution

# Migrar de .sln a .slnx
dotnet sln migrate

# Listar templates disponibles
dotnet new list

# Crear desde template personalizado
dotnet new webapi -n MiApi
```

## Gestionar proyectos en solution

```bash
# Añadir proyecto a solution
dotnet sln MiSolution.slnx add MiProyecto/MiProyecto.csproj
dotnet sln MiSolution.slnx add MiProyecto.Test/MiProyecto.Test.csproj

# Quitar proyecto
dotnet sln MiSolution.slnx remove MiProyecto/MiProyecto.csproj

# Listar proyectos
dotnet sln MiSolution.slnx list
```

## Paquetes NuGet

```bash
# Añadir paquete
dotnet add package Newtonsoft.Json
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package NUnit

# Añadir paquete versión específica
dotnet add package NUnit --version 4.3.2

# Quitar paquete
dotnet remove package Newtonsoft.Json

# Buscar paquete
dotnet package search "json serializer"

# Listar paquetes del proyecto
dotnet list package

# Listar paquetos con vulnerabilidades
dotnet list package --vulnerabilities

# Actualizar paquete
dotnet add package Newtonsoft.Json --version 13.0.3

# Restaurar paquetes (descarga los que faltan)
dotnet restore

# Restaurar con limpieza de caché
dotnet restore --no-cache
```

### Configurar fuentes NuGet (`nuget.config`)

Los paquetes se descargan de **fuentes** (feeds). Por defecto es `nuget.org`, pero puedes añadir repositorios privados o empresariales.

**Ubicación global del usuario:**
```
C:\Users\<TU_USUARIO>\AppData\Roaming\NuGet\nuget.config
```

**Ejemplo de `nuget.config`:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <!-- Fuente empresarial / privada -->
    <add key="mi-empresa" value="https://packages.miempresa.com/repository/nuget/index.json" />
    <!-- GitHub Packages -->
    <add key="github" value="https://nuget.pkg.github.com/MI_USUARIO/index.json" />
  </packageSources>
</configuration>
```

```bash
# Ver fuentes configuradas
dotnet nuget list source

# Añadir fuente
dotnet nuget add source "https://mi-repo.com/index.json" -n mi-repositorio

# Quitar fuente
dotnet nuget remove source mi-repositorio

# Habilitar / deshabilitar fuente
dotnet nuget disable source mi-repositorio
dotnet nuget enable source mi-repositorio

# Verificar conexión con la fuente
dotnet nuget verify MiPaquete
```

> 💡 **Consejo:** Si un `dotnet restore` falla buscando un paquete, comprueba que tu `nuget.config` tiene la fuente correcta.

## Compilar y ejecutar

```bash
# Compilar
dotnet build
dotnet build --configuration Release    # Release

# Ejecutar
dotnet run
dotnet run --project MiProyecto        # especificar proyecto
dotnet run -- --arg1 value1            # pasar argumentos

# Publicar (para despliegue)
dotnet publish -c Release -o ./publish
dotnet publish -c Release --self-contained  # sin necesitar .NET instalado

# Limpiar
dotnet clean
```

## Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con verbosidad
dotnet test --verbosity normal

# Ejecutar test específico
dotnet test --filter "FullyQualifiedName~NombreDelTest"

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests con coverlet y runsettings
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

## Gestionar herramientas globales

```bash
# Listar herramientas instaladas
dotnet tool list -g

# Instalar herramienta
dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-trace
dotnet tool install --global dotnet-reportgenerator-globaltool

# Actualizar herramienta
dotnet tool update -g dotnet-ef

# Desinstalar herramienta
dotnet tool uninstall -g dotnet-ef
```

## Entity Framework Core

```bash
# Instalar EF Core tools
dotnet tool install --global dotnet-ef

# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update

# Eliminar migración
dotnet ef migrations remove

# Ver migraciones pendientes
dotnet ef migrations list

# Script SQL desde migración
dotnet ef migrations script
```

## Herramientas de diagnóstico

```bash
# Instalar herramientas de diagnóstico
dotnet tool install --global dotnet-trace
dotnet tool install --global dotnet-dump
dotnet tool install --global dotnet-counters

# ─── TRACE (grabar perfil de ejecución) ────────────────────────
# Ver procesos .NET en ejecución
dotnet-trace ps

# Grabar trace de un proceso (por PID)
dotnet-trace collect --process-id <PID> -o mi-trace.nettrace

# Grabar con duración máxima (60 segundos)
dotnet-trace collect --process-id <PID> --duration 00:00:60 -o mi-trace.nettrace

# Grabar con eventos específicos (GC, HTTP, EF Core)
dotnet-trace collect --process-id <PID> \
  --providers System.Runtime,Microsoft.EntityFrameworkCore,Microsoft.AspNetCore.Hosting

# Abrir trace en Visual Studio o SpeedScope
# En VS: Archivo → Abrir → Archivo → seleccionar .nettrace
# En navegador: https://www.speedscope.app → arrastrar archivo

# ─── DUMP (volcado de memoria) ────────────────────────────────
# Crear dump de un proceso
dotnet dump collect --process-id <PID>

# Analizar dump
dotnet dump analyze <archivo.dump>
# Dentro del analizador:
#   threads                # ver hilos
#   thread 1               # seleccionar hilo
#   clrstack               # stack CLR del hilo actual
#   clrstack -all          # stack de todos los hilos
#   dumpobject <address>   # inspeccionar objeto
#   exit                   # salir

# ─── COUNTERS (métricas en tiempo real) ────────────────────────
# Monitorizar métricas en vivo
dotnet-counters monitor --process-id <PID>

# Métricas específicas
dotnet-counters monitor --process-id <PID> \
  --counters System.Runtime,Microsoft.AspNetCore.Hosting

# Ejemplo de salida:
#   CPU Usage: 12.3%
#   Working Set: 85 MB
#   Gen 0 Collections: 42
#   Gen 1 Collections: 5
#   Gen 2 Collections: 1
#   Exception Count: 0
```

## Configuración

```bash
# Ver SDK instalado
dotnet --list-sdks

# Ver runtime instalado
dotnet --list-runtimes

# Ver versión
dotnet --version

# Configurar nivel de warnings como errores (en .csproj)
# <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

## Comandos rápidos

```bash
# Crear + compilar + ejecutar (desde cero)
dotnet new console -n Temp && cd Temp && dotnet run

# Buscar en historial de comandos
dotnet --help
dotnet new --help
dotnet run --help
```
