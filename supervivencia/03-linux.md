# Linux — Comandos de Supervivencia

> Comandos esenciales para trabajar en terminales Linux y contenedores Docker.

## Navegación y archivos

```bash
# Directorio actual
pwd

# Listar archivos
ls                    # normal
ls -la                # con detalles + ocultos
ls -lh                # tamaño legible (KB, MB)

# Cambiar directorio
cd /ruta              # absoluto
cd ..                 # subir uno
cd ~                  # ir a home
cd -                  # volver al anterior

# Crear archivos/carpetas
touch archivo.txt
mkdir carpeta
mkdir -p carpeta/sub1/sub2   # crear toda la ruta

# Copiar y mover
cp archivo.txt copia.txt
cp -r carpeta/ destino/      # copiar carpeta entera
mv archivo.txt nuevo.txt     # renombrar
mv archivo.txt /otra/ruta/   # mover

# Eliminar
rm archivo.txt
rm -rf carpeta/              # forzar eliminación recursiva
```

> 🔧 **Truco:** `cd -` vuelve al directorio anterior. Muy útil para ir y volver entre dos carpetas.

> ⚠️ **Advertencia:** `rm -rf` borra SIN confirmación y SIN ir a la papelera. Verifica siempre la ruta antes de ejecutar. Nunca hagas `rm -rf /`.

## Ver contenido de ficheros

```bash
cat archivo.txt              # todo de golpe
less archivo.txt             # paginado (q para salir)
head -20 archivo.txt         # primeras 20 líneas
tail -20 archivo.txt         # últimas 20 líneas
tail -f archivo.txt          # seguir en tiempo real (logs)
wc -l archivo.txt            # contar líneas
```

> 💡 **Consejo:** `tail -f` es tu mejor amigo para ver logs en tiempo real. Combínalo con `grep` para filtrar: `tail -f app.log | grep ERROR`.

> 🔧 **Truco:** `less` es mejor que `cat` para archivos grandes porque puedes navegar con flechas y buscar con `/texto`.

## Buscar

```bash
# Buscar archivos
find . -name "*.cs"                    # por nombre
find . -name "*.cs" -type f            # solo ficheros
find . -mtime -7                       # modificados en últimos 7 días

# Buscar contenido dentro de archivos
grep "texto" archivo.txt
grep -r "texto" ./carpeta/             # recursivo
grep -rn "texto" ./carpeta/            # con número de línea
grep -i "texto" archivo.txt            # sin importar mayúsculas
```

> 💡 **Consejo:** `grep -rn` es perfecto para buscar en qué archivo está una función: `grep -rn "MiFuncion" ./src/`.

> 🔧 **Truco:** `find` + `grep` combinados: `find . -name "*.cs" -exec grep -l "TODO" {} \;` — busca "TODO" en todos los archivos .cs.

## Permisos

```bash
# Ver permisos
ls -l archivo.txt
# -rwxr-xr-- 1 user group 1234 date archivo.txt
#  ↑↑↑↑↑↑↑↑↑  permisos (owner grupo others)

# Cambiar permisos
chmod 755 archivo.sh         # rwxr-xr-x
chmod +x archivo.sh          # añadir ejecución
chmod -R 777 carpeta/        # recursivo (¡cuidado!)

# Cambiar propietario
sudo chown usuario:grupo archivo.txt
```

> 💡 **Analogía:** Los permisos son como una caja fuerte: `r` (leer) = ver el contenido, `w` (escribir) = modificar, `x` (ejecutar) = usar como programa. `755` significa "yo hago todo, los demás leen y ejecutan".

> ⚠️ **Advertencia:** `chmod -R 777` da permisos totales a TODO el mundo. Solo úsalo en entornos de desarrollo, NUNCA en producción.

## Procesos

```bash
# Ver procesos
ps aux                              # todos
ps aux | grep dotnet                # filtrar

# Matar proceso
kill <PID>
kill -9 <PID>                       # forzar

# Buscar por nombre
pkill -f "nombre-proceso"

# Recursos del sistema
top                                 # procesos en tiempo real
htop                                # mejor visualización
free -h                             # memoria
df -h                               # disco
du -sh carpeta/                     # tamaño de carpeta
```

> 💡 **Consejo:** `htop` es mucho más legible que `top`. Si no lo tienes instalado: `sudo apt install htop`.

> 🔧 **Truco:** Para encontrar qué proceso usa un puerto: `lsof -i :5432` o `ss -tuln | grep 5432`.

## Red

```bash
# Ver interfaces de red
ip addr
ifconfig

# Probar conectividad
ping google.com
curl -I https://google.com          # solo cabeceras

# Ver puertos en uso
netstat -tuln
ss -tuln

# DNS
nslookup google.com
dig google.com
```

> 💡 **Consejo:** `ss -tuln` es la versión moderna de `netstat`. Muestra puertos TCP/UDP en escucha sin resolución de nombres (más rápido).

## Compressión y descompresión

```bash
# TAR + GZ
tar -czvf archivo.tar.gz carpeta/    # comprimir
tar -xzvf archivo.tar.gz            # descomprimir

# ZIP
zip -r archivo.zip carpeta/
unzip archivo.zip
```

> 🔧 **Truco:** `tar -czvf` es como hacer un zip pero manteniendo permisos y enlaces. Es el estándar en Linux/Servidores.

## Variables de entorno

```bash
# Ver todas
env

# Ver una
echo $HOME
echo $PATH

# Definir (temporal, solo esta sesión)
export MI_VAR="valor"

# Definir (permanente, en ~/.bashrc)
echo 'export MI_VAR="valor"' >> ~/.bashrc
source ~/.bashrc
```

> ⚠️ **Advertencia:** Las variables definidas con `export` solo viven en la sesión actual. Para que sean permanentes, añádelas a `~/.bashrc` o `~/.bash_profile`.

## Atajos de terminal

| Atajo | Acción |
|-------|--------|
| `Tab` | Autocompletar |
| `Ctrl + C` | Interrumpir proceso |
| `Ctrl + L` | Limpiar pantalla |
| `Ctrl + A` | Ir al inicio de la línea |
| `Ctrl + E` | Ir al final de la línea |
| `Ctrl + R` | Buscar en historial |
| `↑ / ↓` | Navegar historial de comandos |
| `!!` | Repetir último comando |
| `!$` | Último argumento del comando anterior |
