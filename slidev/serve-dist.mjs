// Servidor estático mínimo para servir la versión construida (dist/) de Slidev.
// Uso: node serve-dist.mjs [puerto]   (por defecto 8080)
//
// NECESARIO para ver la presentación en el navegador del entorno: el servidor de
// desarrollo pide ~50 módulos en paralelo y el proxy del navegador falla ~40%
// (502) → nunca arranca. Este servidor sirve la build con cabeceras de CACHÉ:
// cada recarga solo pide lo que aún no bajó bien y la página acaba cargando.
//
// NOTA: es solo para VISUALIZAR. Para medir se usa Playwright (no depende del
// proxy) y para el PDF, `slidev export`.
import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';

const raiz = join(fileURLToPath(new URL('.', import.meta.url)), 'dist');
const puerto = Number(process.argv[2] || 8080);

const mime = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.mjs': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.gif': 'image/gif',
  '.webp': 'image/webp',
  '.ico': 'image/x-icon',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.ttf': 'font/ttf',
  '.map': 'application/json',
  '.txt': 'text/plain; charset=utf-8',
  '.pdf': 'application/pdf',
};

createServer(async (req, res) => {
  try {
    let ruta = decodeURIComponent(new URL(req.url, 'http://x').pathname);
    if (ruta.endsWith('/')) ruta += 'index.html';
    const destino = normalize(join(raiz, ruta));
    if (!destino.startsWith(raiz)) {
      res.writeHead(403).end('403');
      return;
    }
    let archivo = destino;
    try {
      const s = await stat(archivo);
      if (s.isDirectory()) archivo = join(archivo, 'index.html');
    } catch {
      // Ruta tipo /1 (SPA): sirve el index.html y que el router se encargue
      archivo = join(raiz, 'index.html');
    }
    const datos = await readFile(archivo);
    // Caché: el proxy del navegador falla en ráfaga paralela (~40% de 502).
    // Con caché, cada recarga solo pide lo que aún no tiene y acaba cargando.
    const esHtml = extname(archivo).toLowerCase() === '.html';
    res.writeHead(200, {
      'content-type': mime[extname(archivo).toLowerCase()] || 'application/octet-stream',
      'cache-control': esHtml ? 'no-cache' : 'public, max-age=3600',
    });
    res.end(datos);
  } catch (e) {
    res.writeHead(500).end('500: ' + e.message);
  }
}).listen(puerto, () => {
  console.log(`dist en http://localhost:${puerto}/ (IPv4 e IPv6)`);
});
