# Git & GitHub — Comandos de Supervivencia

> Referencia rápida para no perderse con el control de versiones.

## Configuración inicial

```bash
# Configurar usuario (una vez por máquina)
git config --global user.name "Tu Nombre"
git config --global user.email "tu@email.com"

# Ver configuración
git config --list
```

## Crear repositorio

```bash
# Inicializar en carpeta actual
git init

# Clonar existente
git clone https://github.com/usuario/repo.git
git clone https://github.com/usuario/repo.git carpeta-local  # nombre personalizado
```

## Flujo básico (add → commit → push)

```bash
# Ver estado
git status

# Añadir archivos al staging
git add archivo.cs              # uno
git add .                       # todos
git add Models/ Services/       # carpetas específicas
git reset archivo.cs            # quitar del staging

# Commitear
git commit -m "feat: añadir servicio de productos"
git commit -am "fix: corregir error en login"  # add + commit en uno

# Subir al remoto
git push
git push -u origin main         # primera vez, establecer upstream
```

## Pull y sincronización

```bash
# Bajar cambios del remoto
git pull
git pull --rebase               # rebase en vez de merge

# Ver ramas remotas
git branch -r
```

## Ramas

```bash
# Listar ramas
git branch                      # local
git branch -a                   # todas (local + remota)

# Crear rama
git branch feature/nuevo-servicio

# Cambiar de rama
git checkout feature/nuevo-servicio
git checkout -b feature/nuevo-servicio  # crear + cambiar en uno

# Renombrar rama
git branch -m nombre-viejo nombre-nuevo

# Eliminar rama
git branch -d rama-local         # segura
git branch -D rama-local         # forzar

# Eliminar rama remota
git push origin --delete rama-remota
```

## Merge y rebase

```bash
# Merge (crear commit de merge)
git checkout main
git merge feature/nuevo-servicio

# Rebase (reencadenar commits)
git checkout feature/nuevo-servicio
git rebase main
```

## Ver historial

```bash
git log                         # completo
git log --oneline               # compacto
git log --oneline --graph       # con gráfico de ramas
git log --since="2 weeks ago"   # por fecha
git diff                        # cambios sin staging
git diff --staged               # cambios en staging
```

## Deshacer

```bash
# Deshacer último commit (mantener cambios)
git reset --soft HEAD~1

# Deshacer último commit (perder cambios)
git reset --hard HEAD~1

# Deshacer cambios en archivo
git checkout -- archivo.cs

# Ver historial para recuperar algo
git reflog
git checkout <commit-hash>      # recuperar archivo puntual
```

## GitHub

```bash
# Añadir remoto
git remote add origin https://github.com/usuario/repo.git

# Ver remotos
git remote -v

# Crear repositorio en GitHub (desde CLI)
gh repo create mi-repo --public
gh repo create mi-repo --private
```

## Errores comunes

| Error | Solución |
|-------|----------|
| `fatal: not a git repository` | `git init` en la carpeta correcta |
| `rejected (fetch)` | `git pull --rebase` antes de push |
| `detached HEAD` | `git checkout main` para volver |
| `merge conflict` | Editar archivo, marcar resuelto, `git add` + `git commit` |
| `remote: Permission denied` | Revisar credenciales o token de acceso |

## .gitignore básico (para C#)

```
bin/
obj/
*.user
*.suo
.vs/
.vscode/
*.log
TestResults/
publish/
```
