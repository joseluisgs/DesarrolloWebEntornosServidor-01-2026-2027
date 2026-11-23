<script setup lang="ts">
/**
 * Resultados de aprendizaje (RAA) y criterios de evaluación (CCEE)
 * de la UD — se muestra en slide propia, nunca en la portada.
 */
defineProps<{
  code: string
  title: string
  ccee: string[]
  /**
   * Letras explícitas de cada CCEE (p. ej. ['a', 'g']). Si no se pasan,
   * se generan en secuencia a), b), c)… — pero OJO: las letras reales son
   * las del README de la unidad (no siempre son consecutivas).
   */
  letters?: string[]
}>()
</script>

<template>
  <div class="ud-raa">
    <div class="ud-raa-box">
      <span class="ud-raa-badge">{{ code }}</span>
      <p class="ud-raa-title">{{ title }}</p>
    </div>
    <div class="ud-raa-ccee">
      <div class="ud-raa-ccee-head">Criterios de evaluación (CCEE)</div>
      <div class="ud-raa-grid" :class="{ 'ud-raa-grid--three': ccee.length === 3 }">
        <div v-for="(c, i) in ccee" :key="i" class="ud-raa-item" v-click>
          <span class="ud-raa-letter">{{ (letters && letters[i]) || String.fromCharCode(97 + i) }})</span>
          <span>{{ c }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ud-raa {
  display: flex;
  flex-direction: column;
  gap: 1em;
  text-align: left;
}

.ud-raa-box {
  display: flex;
  align-items: center;
  gap: 0.9em;
  background: var(--ud-accent-soft);
  border: 1px solid var(--ud-accent);
  border-radius: var(--ud-radius);
  padding: 0.8em 1.1em;
}

.ud-raa-badge {
  flex: none;
  background: var(--ud-accent);
  color: var(--ud-on-accent);
  font-weight: 700;
  border-radius: 8px;
  padding: 0.35em 0.7em;
  font-size: 0.95em;
}

.ud-raa-title {
  margin: 0;
  color: var(--ud-text);
  font-weight: 600;
  font-size: 0.95em;
  line-height: 1.4;
}

.ud-raa-ccee-head {
  font-weight: 700;
  color: var(--ud-text);
  font-size: 0.9em;
  margin-bottom: 0.5em;
}

.ud-raa-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5em;
}

.ud-raa-item:last-child:nth-child(3n + 1) {
  grid-column: 1 / -1;
}

.ud-raa-grid--three {
  grid-template-columns: 1fr;
  gap: 0.65em;
}

.ud-raa-grid--three .ud-raa-item {
  padding: 0.65em 0.8em;
  font-size: 0.9em;
}

.ud-raa-item {
  display: flex;
  align-items: flex-start;
  gap: 0.5em;
  background: var(--ud-bg-elv);
  border: 1px solid var(--ud-divider);
  border-radius: 10px;
  padding: 0.5em 0.65em;
  font-size: 0.75em;
  line-height: 1.4;
  color: var(--ud-text-mute);
  box-shadow: var(--ud-shadow);
}

.ud-raa-letter {
  flex: none;
  display: grid;
  place-items: center;
  width: 1.6em;
  height: 1.6em;
  border-radius: 6px;
  background: var(--ud-accent-soft);
  border: 1px solid var(--ud-accent);
  color: var(--ud-accent-strong);
  font-weight: 700;
  font-family: var(--ud-font-mono);
}

html.dark .ud-raa-letter {
  color: #a3b1ee;
}
</style>
