# Windows — Comandos de Supervivencia

> Comandos de PowerShell y CMD para desarrollo en Windows.

## PowerShell vs CMD

| Acción | PowerShell | CMD |
|--------|-----------|-----|
| Ayuda | `Get-Help` | `help` |
| Limpiar pantalla | `Clear-Host` | `cls` |
| Cambiar directorio | `Set-Location` o `cd` | `cd` |
| Variables | `$variable` | `%variable%` |

## Navegación y archivos (PowerShell)

```powershell
# Directorio actual
Get-Location
pwd

# Listar archivos
Get-ChildItem
Get-ChildItem -Path . -Recurse -Filter "*.cs"   # recursivo con filtro
Get-ChildItem -Directory                         # solo carpetas
Get-ChildItem -File                              # solo ficheros
ls                                                # alias rápido

# Crear archivos/carpetas
New-Item -ItemType Directory -Path "carpeta"
New-Item -ItemType File -Path "archivo.txt"

# Copiar y mover
Copy-Item archivo.txt destino.txt
Copy-Item -Recurse carpeta/ destino/
Move-Item archivo.txt nuevo.txt

# Eliminar
Remove-Item archivo.txt
Remove-Item -Recurse -Force carpeta/    # forzar eliminación
```

## Ver contenido

```powershell
Get-Content archivo.txt                  # todo
Get-Content archivo.txt -Head 20         # primeras 20 líneas
Get-Content archivo.txt -Tail 20         # últimas 20 líneas
type archivo.txt                         # CMD
```

## Buscar

```powershell
# Buscar archivos
Get-ChildItem -Recurse -Filter "*.cs"

# Buscar contenido
Select-String -Path "*.cs" -Pattern "texto"
Select-String -Path "*.cs" -Pattern "texto" -CaseSensitive

# Buscar en múltiples archivos
Get-ChildItem -Recurse -Filter "*.cs" | Select-String "PersonaService"
```

## Procesos

```powershell
# Ver procesos
Get-Process

# Buscar proceso
Get-Process -Name "dotnet"

# Matar proceso
Stop-Process -Name "dotnet" -Force
Stop-Process -Id 1234                   # por PID
```

## Red

```powershell
# Ver configuración de red
Get-NetIPAddress

# Probar conectividad
Test-Connection google.com
Test-NetConnection -ComputerName localhost -Port 5432

# Ver puertos en uso
Get-NetTCPConnection | Where-Object { $_.LocalPort -eq 5432 }
```

## Servicios y Docker

```powershell
# Ver servicios
Get-Service

# Docker
docker ps
docker compose up -d
docker compose down
```

## Gestión de paquetes

```powershell
# winget (Windows Package Manager)
winget install JetBrains.Rider
winget upgrade --all
winget list

# Chocolatey (si está instalado)
choco install dotnet-sdk
choco upgrade all
```

## Variables de entorno

```powershell
# Ver variable
$env:PATH

# Definir (solo esta sesión)
$env:MI_VAR = "valor"

# Definir (permanente, usuario)
[System.Environment]::SetEnvironmentVariable("MI_VAR", "valor", "User")
```

## Atajos de PowerShell

| Atajo | Acción |
|-------|--------|
| `Tab` | Autocompletar |
| `Ctrl + C` | Interrumpir |
| `Ctrl + L` | Limpiar pantalla |
| `F7` | Historial de comandos |
| `Ctrl + R` | Buscar en historial |
| `Ctrl + Space` | Intellisense |
| `F8` | Buscar en historial (por texto) |

## Comandos útiles de desarrollo

```powershell
# Ver qué usa el puerto 5432
netstat -ano | findstr :5432

# Matar proceso que usa un puerto
Stop-Process -Id (Get-NetTCPConnection -LocalPort 5432).OwningProcess -Force

# Ver variable de entorno del sistema
[System.Environment]::GetEnvironmentVariable("PATH", "Machine")

# Ejecutar como administrador
Start-Process powershell -Verb RunAs
```
