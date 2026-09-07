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

> 🔧 **Truco:** Usa `git config --global core.autocrlf true` en Windows para evitar problemas con saltos de línea.

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

> 💡 **Analogía:** `git add` es como meter algo en la caja de "listo para enviar". `git commit` cierra la caja con un mensaje. `git push` la envía al almacén (remoto).

> 💡 **Consejo:** Los mensajes de commit en inglés siguen la convención: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`. Si no estás seguro, mira los commits anteriores con `git log --oneline`.

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

> 💡 **Consejo:** Nombrea las ramas con prefijo: `feature/`, `fix/`, `hotfix/`, `chore/`. Ejemplo: `feature/carrito-compra`.

> ⚠️ **Advertencia:** Nunca hagas `git push --force` en ramas compartidas (`main`, `develop`). Puedes borrar el trabajo de otros.

## Merge y rebase

```bash
# Merge (crear commit de merge)
git checkout main
git merge feature/nuevo-servicio

# Rebase (reencadenar commits)
git checkout feature/nuevo-servicio
git rebase main
```

> 💡 **Consejo:** Usa `rebase` para mantener un historial limpio (sin merges innecesarios). Usa `merge` cuando quieras preservar el contexto de una rama completa.

> 🔧 **Truco:** Si hay conflictos en rebase, resuélvelos y luego `git rebase --continue` (no `--abort` a menos que quieras empezar de cero).

## Ver historial

```bash
git log                         # completo
git log --oneline               # compacto
git log --oneline --graph       # con gráfico de ramas
git log --since="2 weeks ago"   # por fecha
git diff                        # cambios sin staging
git diff --staged               # cambios en staging
```

## Deshacer cambios

```bash
# ─── EN EL STAGING (ya hiciste git add) ────────────────────────
# Quitar archivo del staging (mantiene cambios en disco)
git restore archivo.cs
git restore Models/             # una carpeta entera

# Quitar todo del staging
git restore .

# ─── MODIFICAR ÚLTIMO COMMIT ───────────────────────────────────
# Cambiar el mensaje del último commit
git commit --amend -m "nuevo mensaje"

# Añadir archivos olvidados al último commit
git add archivos-olvidados.cs
git commit --amend --no-edit   # sin cambiar mensaje

# ─── RESET (deshacer commits) ─────────────────────────────────
# --soft: deshace commit, mantiene staging + archivos
git reset --soft HEAD~1

# --mixed (por defecto): deshace commit + staging, mantiene archivos
git reset HEAD~1
git reset --mixed HEAD~1       # lo mismo

# --hard: deshace TODO (commit + staging + archivos) ← ¡CUIDADO!
git reset --hard HEAD~1

# Ir a un commit concreto (mantiene archivos)
git reset --soft <commit-hash>

# ─── REVERT (deshacer un commit con commit nuevo) ─────────────
# Crea un commit nuevo que invierte los cambios (seguro para shared)
git revert <commit-hash>
git revert HEAD                # revert del último commit

# ─── STASH (guardar cambios temporalmente) ─────────────────────
# Guardar cambios sin commitear
git stash
git stash push -m "cambios parciales del servicio"

# Listar stashes
git stash list

# Recuperar último stash (mantiene el stash)
git stash pop

# Recuperar y eliminar el stash
git stash apply stash@{0}
git stash drop stash@{0}

# Limpiar todos los stashes
git stash clear
```

> 💡 **Analogía:** `stash` es como hacer una " pausa" y guardar lo que tienes en la mesa en un cajón. Cuando vuelves, sacas lo que guardaste con `pop`.

> ⚠️ **Advertencia:** `git stash clear` borra TODO sin confirmación. Usa `git stash drop stash@{0}` para borrar uno concreto.

# ─── REFLOG (recuperar lo que parece perdido) ──────────────────
# Ver historial de movimientos de HEAD
git reflog

# Recuperar un commit "perdido"
git checkout <commit-hash>
git branch recuperacion        # crear rama para guardarlo
```

> ⚠️ **Advertencia:** `git reset --hard` borra tus cambios sin posibilidad de recuperación (salvo con `reflog`). Úsalo solo si estás seguro.

## GitHub

```bash
# ─── CONFIGURACIÓN ─────────────────────────────────────────────
# Autenticar con GitHub (abre navegador)
gh auth login

# Ver estado de autenticación
gh auth status

# ─── REPOSITORIOS ──────────────────────────────────────────────
# Crear repositorio
gh repo create mi-repo --public
gh repo create mi-repo --private
gh repo create mi-repo --private --source=. --push   # crear + subir

# Clonar
gh repo clone usuario/repo

# Ver info del repo
gh repo view

# Abrir en navegador
gh browse

# ─── ISSUES ────────────────────────────────────────────────────
# Crear issue
gh issue create --title "Error en login" --body "Descripción del bug"
gh issue create -t "Nueva feature" -b "Implementar X" -l "enhancement"

# Listar issues
gh issue list
gh issue list --state open
gh issue list --label "bug"

# Ver issue
gh issue view 42

# Cerrar issue
gh issue close 42

# Asignar issue
gh issue edit 42 --add-assignee @me
```

> 💡 **Consejo:** Enlaza un PR con un issue usando `Fix #42` en la descripción. Cuando el PR se merge, el issue se cierra automáticamente.

> 🔧 **Truco:** `gh issue create` abre el editor para escribir la descripción. Si prefieres todo desde terminal, usa `--body "texto"`.

```bash
# ─── PULL REQUESTS ─────────────────────────────────────────────
# Crear PR
gh pr create --title "feat: añadir servicio" --body "Descripción"
gh pr create -t "fix: corregir bug" -b "Fix #42" -r "otrebor,otro-reviewer"

# Listar PRs
gh pr list
gh pr list --state open

# Ver PR
gh pr view 15

# Ver diff del PR
gh pr diff 15

# Review (aprobar / solicitar cambios)
gh pr review 15 --approve
gh pr review 15 --request-changes --comment "Faltan tests"

# Merge
gh pr merge 15 --merge       # merge commit
gh pr merge 15 --squash      # squash (un solo commit)
gh pr merge 15 --rebase      # rebase

# Checkout de un PR localmente
gh pr checkout 15

# Abrir PR en navegador
gh pr browse 15
```

> 💡 **Consejo:** `--squash` es útil para limpiar commits molestos ("fix typo", "wip") antes de merge a `main`. `--rebase` mantiene el historial lineal.

> ⚠️ **Advertencia:** Revisa SIEMPRE el diff de un PR antes de approve: `gh pr diff <número>`.

# ─── RELEASES ──────────────────────────────────────────────────
# Listar releases
gh release list

# Crear release
gh release create v1.0.0 --title "v1.0.0" --notes "Primera versión"

# Descargar release
gh release download v1.0.0

# ─── WORKFLOWS (GitHub Actions) ───────────────────────────────
# Ver workflows
gh workflow list

# Ver ejecuciones
gh run list

# Ver log de una ejecución
gh run view <run-id> --log
```

## Errores comunes

| Error | Solución |
|-------|----------|
| `fatal: not a git repository` | `git init` en la carpeta correcta |
| `rejected (fetch)` | `git pull --rebase` antes de push |
| `detached HEAD` | `git checkout main` para volver |
| `merge conflict` | Editar archivo, marcar resuelto, `git add` + `git commit` |
| `remote: Permission denied` | Revisar credenciales o token de acceso |
| `error: failed to push some refs` | `git pull --rebase origin main` y reintentar push |

> 💡 **Consejo:** Si ves "Detached HEAD", no es un error grave — es que estás en un commit sin rama. Crea una rama con `git switch -c nueva-rama` para no perder los cambios.

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
