<template>
  <div class="ud-brand-footer">
    <span class="ud-brand-footer-left">UD01 · Introducción al desarrollo web en servidor</span>
    <span class="ud-brand-footer-right">Desarrollo Web en Entorno Servidor · 2DAW · IES Luis Vives · joseluisgs.dev</span>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';

/* ============================================================
   Ajuste de Mermaid: el SVG vive en un shadowRoot, así que el CSS
   externo NO llega a él y el host lo corta con max-height + overflow
   (bug: "diagrama recortado por abajo, nodo a medias").
   Solución: cuando el SVG supera el alto del host, reducimos SU ANCHO
   proporcionalmente; como lleva viewBox, el alto baja solo y el
   diagrama queda COMPLETO (sin recorte) y centrado.
   Nunca se agranda: solo se estrecha.
   ============================================================ */
const RESERVA_ALTO = 350; // respaldo si el host no tiene max-height

function ajustarMermaid(host: Element): void {
  const raiz = (host as Element & { shadowRoot: ShadowRoot | null }).shadowRoot;
  const svg = raiz?.querySelector('svg');
  if (!svg) return;
  // Borde fino de marca en los clusters, sin sombra: se fija en línea para
  // que gane siempre a cualquier regla inyectada por el look de Mermaid.
  raiz.querySelectorAll<SVGRectElement>('.cluster rect').forEach((r) => {
    if (r.style.stroke !== '#7289da') r.style.stroke = '#7289da';
    if (r.style.fill !== 'transparent') r.style.fill = 'transparent';
    if (r.style.filter !== 'none') r.style.filter = 'none';
  });
  // El SVG sale con display:inline (defecto) y la línea de texto le añade
  // ~6px de espacio bajo la línea de base → el host se pasa y se corta.
  // En bloque desaparece ese gap. Además lo centramos: los diagramas
  // estrechos se quedaban pegados a la izquierda con un hueco enorme.
  if (svg.style.display !== 'block') svg.style.display = 'block';
  if (svg.style.marginInline !== 'auto') svg.style.marginInline = 'auto';

  /* --------------------------------------------------------------
     Cálculo ESTABLE: nunca a partir del tamaño actual del SVG (que
     ya lleva nuestro propio estilo) sino del viewBox y de la caja del
     host. Así la segunda pasada calcula lo MISMO y no vuelve a
     escribir: si no, el observador dispara otra pasada → bucle
     infinito → el navegador se queda colgado.
     -------------------------------------------------------------- */
  const vb = (svg as SVGSVGElement).viewBox?.baseVal;
  const vbW = vb?.width || 0;
  const vbH = vb?.height || 0;
  if (!vbW || !vbH) return;
  const ratio = vbW / vbH;

  const hostW = host.clientWidth;
  if (!hostW) return; // aún no se ha pintado

  const maxCss = parseFloat(getComputedStyle(host).maxHeight) || RESERVA_ALTO;
  let tope = maxCss;

  const cont = host.parentElement;
  if (cont && getComputedStyle(cont).display.includes('flex')) {
    // Hueco libre real en el contenedor (sin tocar los márgenes del host).
    let libre = cont.clientHeight;
    for (const h of Array.from(cont.children)) {
      if (h === host) continue;
      const hs = getComputedStyle(h);
      libre -= h.offsetHeight + (parseFloat(hs.marginTop) || 0) + (parseFloat(hs.marginBottom) || 0);
    }
    const ms = getComputedStyle(host);
    libre -= (parseFloat(ms.marginTop) || 0) + (parseFloat(ms.marginBottom) || 0);
    // OJO: NUNCA usar la caja del host como límite: su alto depende del SVG
    // que acabamos de encoger → se retroalimenta y el diagrama se queda
    // clavado en el mínimo (bug: "diagrama enanito"). `libre` ya es estable.
    tope = Math.max(40, Math.min(maxCss, libre));
  }

  const ancho = Math.max(40, Math.min(hostW, Math.floor(tope * ratio)));
  const alto = Math.floor(ancho / ratio);
  // Escritura SOLO si el valor cambió (evita el bucle de mutaciones).
  if (svg.style.width !== ancho + 'px') {
    svg.style.width = ancho + 'px';
    svg.style.height = alto + 'px';
    svg.style.maxWidth = ''; // limpia restos de versiones anteriores
  }
}

function barrerMermaid(): void {
  document.querySelectorAll('.mermaid').forEach((host) => {
    ajustarMermaid(host);
    const raiz = (host as Element & { shadowRoot: ShadowRoot | null }).shadowRoot;
    if (raiz && !host.hasAttribute('data-ud-obs')) {
      host.setAttribute('data-ud-obs', '1');
      // El SVG se pinta de forma asíncrona: observamos su shadowRoot.
      // Observamos también el atributo style: al estrechar el SVG vuelve a
      // correr el ajuste (converge: solo se estrecha, nunca se agranda).
      new MutationObserver(() => ajustarMermaid(host)).observe(raiz, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['style'],
      });
    }
  });
}

onMounted(() => {
  barrerMermaid();
  // Mermaid puede tardar en renderizar tras montar la slide
  setTimeout(barrerMermaid, 250);
  setTimeout(barrerMermaid, 800);
});
</script>

<style scoped>
.ud-brand-footer {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.35em 1.5em;
  font-size: 0.6em;
  letter-spacing: 0.02em;
  color: var(--ud-text-subtle);
  border-top: 1px solid var(--ud-divider);
  background: transparent;
  pointer-events: none;
  z-index: 20;
}
</style>
