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

## Ver contenido de ficheros

```bash
cat archivo.txt              # todo de golpe
less archivo.txt             # paginado (q para salir)
head -20 archivo.txt         # primeras 20 líneas
tail -20 archivo.txt         # últimas 20 líneas
tail -f archivo.txt          # seguir en tiempo real (logs)
wc -l archivo.txt            # contar líneas
```

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

## Compressión y descompresión

```bash
# TAR + GZ
tar -czvf archivo.tar.gz carpeta/    # comprimir
tar -xzvf archivo.tar.gz            # descomprimir

# ZIP
zip -r archivo.zip carpeta/
unzip archivo.zip
```

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
