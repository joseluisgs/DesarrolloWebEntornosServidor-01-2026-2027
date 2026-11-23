<script setup lang="ts">
/**
 * Ventana de estilo "consola" para envolver bloques de código
 * (inspirado en el layout text-window de slidev-theme-penguin).
 */
defineProps<{
  title?: string
  file?: string
}>()
</script>

<template>
  <div class="ud-window">
    <div class="ud-window-bar">
      <span class="ud-window-dot ud-window-dot--red" />
      <span class="ud-window-dot ud-window-dot--yellow" />
      <span class="ud-window-dot ud-window-dot--green" />
      <span class="ud-window-title">{{ file || title || 'Program.cs' }}</span>
    </div>
    <div class="ud-window-body">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ud-window {
  border-radius: var(--ud-radius);
  overflow: hidden;
  border: 1px solid var(--ud-divider);
  box-shadow: var(--ud-shadow-lg);
  background: var(--ud-bg-elv);
  text-align: left;
}

.ud-window-bar {
  display: flex;
  align-items: center;
  gap: 0.45em;
  padding: 0.5em 0.9em;
  background: var(--ud-bg-alt);
  border-bottom: 1px solid var(--ud-divider);
}

.ud-window-dot {
  width: 0.7em;
  height: 0.7em;
  border-radius: 50%;
}

.ud-window-dot--red { background: #e0575b; }
.ud-window-dot--yellow { background: #c28100; }
.ud-window-dot--green { background: #30a46c; }

.ud-window-title {
  margin-left: 0.6em;
  font-size: 0.75em;
  color: var(--ud-text-subtle);
  font-family: var(--ud-font-mono);
}

.ud-window-body :deep(pre) {
  margin: 0;
  border: none;
  border-radius: 0;
  box-shadow: none;
}
</style>
