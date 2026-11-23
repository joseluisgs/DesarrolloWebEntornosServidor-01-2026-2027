<script setup lang="ts">
/**
 * 📌 Ejemplo real: ancla el concepto con una aplicación conocida.
 */
defineProps<{
  app: string
}>()
</script>

<template>
  <div class="ud-real">
    <span class="ud-real-label">📌 Ejemplo real:</span>
    <b class="ud-real-app">{{ app }}</b>
    <div class="ud-real-body">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ud-real {
  background: var(--ud-accent-soft);
  border: 1px dashed var(--ud-accent);
  border-radius: var(--ud-radius);
  padding: 0.65em 1em;
  font-size: 0.88em;
  text-align: left;
  color: var(--ud-text-mute);
}

.ud-real-label {
  color: var(--ud-accent-strong);
  font-weight: 600;
}

html.dark .ud-real-label {
  color: #a3b1ee;
}

.ud-real-app {
  color: var(--ud-text);
  margin-left: 0.3em;
}

.ud-real-body {
  margin-top: 0.2em;
}

.ud-real-body :deep(p) {
  margin: 0.15em 0;
}
</style>
