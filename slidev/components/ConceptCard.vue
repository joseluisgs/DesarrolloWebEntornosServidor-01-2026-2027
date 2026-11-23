<script setup lang="ts">
/**
 * Tarjeta de concepto con icono: para grids de ideas clave.
 */
defineProps<{
  icon?: string
  title: string
  accent?: 'accent' | 'green' | 'red' | 'yellow' | 'blue' | 'purple'
}>()
</script>

<template>
  <div class="ud-concept" :class="`ud-concept--${accent || 'accent'}`">
    <div v-if="icon" class="ud-concept-icon">{{ icon }}</div>
    <div class="ud-concept-title">{{ title }}</div>
    <div class="ud-concept-body">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ud-concept {
  background: var(--ud-bg-elv);
  border: 1px solid var(--ud-divider);
  border-top: 3px solid var(--ud-accent);
  border-radius: var(--ud-radius);
  padding: 0.8em 1em;
  box-shadow: var(--ud-shadow);
  text-align: left;
  height: 100%;
}

.ud-concept--green { border-top-color: var(--ud-green); }
.ud-concept--red { border-top-color: var(--ud-red); }
.ud-concept--yellow { border-top-color: var(--ud-yellow); }
.ud-concept--blue { border-top-color: var(--ud-blue); }
.ud-concept--purple { border-top-color: var(--ud-purple); }

.ud-concept-icon {
  font-size: 1.6em;
  line-height: 1;
  margin-bottom: 0.3em;
}

.ud-concept-title {
  font-weight: 700;
  color: var(--ud-text);
  font-size: 0.98em;
  margin-bottom: 0.25em;
}

.ud-concept-body {
  color: var(--ud-text-mute);
  font-size: 0.82em;
  line-height: 1.5;
}

.ud-concept-body :deep(p) {
  margin: 0.15em 0;
}
</style>
