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

# Actualizar paquete
dotnet add package Newtonsoft.Json --version 13.0.3
```

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
# Trace (diagnóstico)
dotnet tool install --global dotnet-trace
dotnet-trace collect --process-id <PID>

# Dump (volcado de memoria)
dotnet dump collect --process-id <PID>
dotnet dump analyze <dump-file>

#-counters (métricas en tiempo real)
dotnet-counters monitor --process-id <PID>
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
