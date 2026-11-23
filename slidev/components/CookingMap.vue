<script setup lang="ts">
/**
 * Metáfora de la cocina: hilo conductor de la UD01.
 * Vincula cada concepto de programación con su equivalente culinario.
 */
const rows = [
  { icon: '🧂', concept: 'Tipos de datos', kitchen: 'Ingredientes: harina, huevo, aceite...' },
  { icon: '🍲', concept: 'Variables', kitchen: 'Cazuelas y recipientes que cambian' },
  { icon: '📜', concept: 'Constantes', kitchen: 'Receta fija: la sal nunca cambia' },
  { icon: '🔪', concept: 'Operadores', kitchen: 'Utensilios: cortar, mezclar, hornear' },
  { icon: '🧾', concept: 'Entrada de datos', kitchen: 'El pedido del cliente' },
  { icon: '🔥', concept: 'Proceso', kitchen: 'La cocción: transformar lo crudo' },
  { icon: '🍽️', concept: 'Salida de datos', kitchen: 'El plato servido' },
  { icon: '📖', concept: 'Algoritmo', kitchen: 'La receta: pasos ordenados' },
  { icon: '🛠️', concept: 'Métodos / Funciones', kitchen: 'Técnicas reutilizables' },
  { icon: '📝', concept: 'Comentarios', kitchen: 'Notas del chef' },
]
</script>

<template>
  <div class="ud-cooking">
    <div
      v-for="(row, i) in rows"
      :key="i"
      class="ud-cooking-item"
    >
      <span class="ud-cooking-icon">{{ row.icon }}</span>
      <div class="ud-cooking-text">
        <b>{{ row.concept }}</b>
        <span class="ud-cooking-arrow"> → </span>
        <span class="ud-cooking-kitchen">{{ row.kitchen }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ud-cooking {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.5em;
  text-align: left;
  /* Nunca a sangre: el mapa se mantenía dentro de los márgenes de la slide
     y se metía debajo del pie (bug: "la imagen invade el footer") */
  width: 100%;
  max-width: 900px;
  margin-inline: auto;
  padding-bottom: 0.6em;
}

.ud-cooking-item {
  display: flex;
  align-items: center;
  gap: 0.6em;
  background: var(--ud-bg-elv);
  border: 1px solid var(--ud-divider);
  border-radius: 10px;
  padding: 0.45em 0.75em;
  font-size: 0.8em;
  box-shadow: var(--ud-shadow);
}

.ud-cooking-icon {
  font-size: 1.3em;
  line-height: 1;
}

.ud-cooking-text {
  line-height: 1.35;
  color: var(--ud-text);
}

.ud-cooking-arrow {
  color: var(--ud-accent);
  font-weight: 700;
}

.ud-cooking-kitchen {
  color: var(--ud-text-mute);
}
</style>
