# .NET CLI — Comandos de Supervivencia

> Referencia rápida de `dotnet` para desarrollo en C#/.NET.

## Crear proyectos

```bash
# Crear proyecto de consola
dotnet new console -n MiProyecto
dotnet new console -n MiProyecto -f net10.0

# Crear proyecto de librería
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

> 💡 **Consejo:** En .NET 10+ el formato de solución por defecto es `.slnx` (XML), no el antiguo `.sln`. Si tienes ambos, especifica cuál usar.

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

> 💡 **Consejo:** `dotnet run` compila y ejecuta en un solo paso. Para desarrollo rápido es perfecto. Para producción, usa `dotnet publish`.

> 🔧 **Truco:** `dotnet run --project` te permite ejecutar un proyecto sin estar en su carpeta. Ideal cuando tienes una solution con varios proyectos.

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

> 💡 **Consejo:** El filtro `FullyQualifiedName~` usa parcial del nombre. Si tu test se llama `Crear_PersonaValida_RetornaId`, puedes filtrar con `--filter "Crear_Persona"`.

> ⚠️ **Advertencia:** Si los tests no se ejecutan, comprueba que el proyecto de test tiene referencia al proyecto principal con `dotnet add reference ../MiProyecto/MiProyecto.csproj`.

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

> 💡 **Consejo:** El nombre de la migración debe ser descriptivo. Usa `dotnet ef migrations add AddTablaProductos`, no `dotnet ef migrations add Migracion1`.

> 🔧 **Truco:** `dotnet ef migrations script` genera un SQL limpio que puedes ejecutar en producción sin arrancar la app.

> ⚠️ **Advertencia:** NUNCA hagas `dotnet ef database update` en producción sin antes probar en local. Y NUNCA uses `dotnet ef migrations remove` en un entorno con datos reales.

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
```

> 💡 **Consejo:** Para encontrar el PID de tu app, usa `dotnet-trace ps` o `Get-Process dotnet` en PowerShell.

> 🔧 **Truco:** SpeedScope es gratis y te muestra un flame graph interactivo. Mucho más claro que leer logs de rendimiento.

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
```

> ⚠️ **Advertencia:** `dotnet dump collect` puede pesar varios MB. No lo dejes corriendo en producción — solo úsalo para diagnosticar un problema concreto.

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

> 💡 **Consejo:** Si `dotnet --version` muestra algo viejo, puede que tengas varios SDKs instalados. Comprueba con `dotnet --list-sdks` y usa `global.json` para fijar la versión.

> 🔧 **Truco:** Para configurar el nivel de warnings en todo el proyecto, añade `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` en el `.csproj`. Así compilerá como errores lo que normalmente son warnings — ¡buen hábito!

## Comandos rápidos

```bash
# Crear + compilar + ejecutar (desde cero)
dotnet new console -n Temp && cd Temp && dotnet run

# Buscar en historial de comandos
dotnet --help
dotnet new --help
dotnet run --help
```

> 💡 **Analogía:** `dotnet --help` es como la wikipedia de cada comando. Si no recuerdas un flag, ¡míralo ahí! Es mejor que buscar en Google.
