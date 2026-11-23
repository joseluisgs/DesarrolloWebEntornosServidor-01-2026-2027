import type { MermaidConfig } from 'mermaid'

/**
 * Configuración de Mermaid para la presentación de la UD.
 *
 * `useMaxWidth: true` es la clave: hace que el SVG ocupe el 100% del hueco
 * que le toca en la slide. Sin esto, Mermaid le pone al SVG un ancho fijo
 * (el del diagrama) y el diagrama se sale por la derecha de la transparencia.
 */
export default (): MermaidConfig => ({
  theme: 'default',
  startOnLoad: false,
  securityLevel: 'loose',
  // 'classic': bordes FINOS y sin sombra. El look por defecto ('neo' desde
  // mermaid 11) pinta nodos y clusters con borde morado + drop-shadow
  // (bug: "los diagramas tienen bordes raros").
  look: 'classic',
  flowchart: {
    useMaxWidth: true,
    htmlLabels: true,
    curve: 'basis',
    // Compacto: los diagramas tienen que caber SOLOS por debajo del tope
    // del host (350px) sin depender del JS de ajuste (si no, al entrar en
    // la slide se ve cortado hasta que refrescas).
    nodeSpacing: 6,
    rankSpacing: 24,
    padding: 4,
  },
  sequence: {
    useMaxWidth: true,
  },
  er: {
    useMaxWidth: true,
  },
  gantt: {
    useMaxWidth: true,
  },
  journey: {
    useMaxWidth: true,
  },
  pie: {
    useMaxWidth: true,
  },
  themeVariables: {
    // Legible en pantalla y en PDF: 15px se veía diminuto en los diagramas
    // pequeños (bug: "nodos con texto de 8px")
    fontSize: '19px',
    fontFamily: "'Ubuntu', 'Segoe UI', system-ui, sans-serif",
    // Subgraf sin fondo (quejado en la 18) pero SÍ con borde fino de marca:
    // caja limpia, sin relleno gris ni sombra
    clusterBkg: 'transparent',
    clusterBorder: '#7289da',
  },
})
