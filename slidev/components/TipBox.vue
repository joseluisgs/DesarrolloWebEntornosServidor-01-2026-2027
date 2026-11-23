<script setup lang="ts">
/**
 * Bloque didáctico con los matices del material de la UD:
 * Consejo, Truco, Advertencia, Nota y Analogía.
 */
const props = withDefaults(
  defineProps<{
    type?: 'consejo' | 'truco' | 'advertencia' | 'nota' | 'analogia'
    title?: string
  }>(),
  { type: 'consejo', title: '' },
)

const meta = {
  consejo: { emoji: '💡', label: 'Consejo', color: 'var(--ud-yellow)', bg: 'var(--ud-yellow-soft)' },
  truco: { emoji: '🔧', label: 'Truco', color: 'var(--ud-blue)', bg: 'var(--ud-blue-soft)' },
  advertencia: { emoji: '⚠️', label: 'Advertencia', color: 'var(--ud-red)', bg: 'var(--ud-red-soft)' },
  nota: { emoji: '📝', label: 'Nota', color: 'var(--ud-accent)', bg: 'var(--ud-accent-soft)' },
  analogia: { emoji: '🧠', label: 'Analogía', color: 'var(--ud-purple)', bg: 'var(--ud-purple-soft)' },
} as const

// Respaldo ante un tipo desconocido: nunca dejar que la slide revente
const current = meta[props.type] ?? meta.consejo
</script>

<template>
  <div
    class="ud-tip"
    :style="{ '--tip-color': current.color, '--tip-bg': current.bg }"
  >
    <span class="ud-tip-head">
      <span class="ud-tip-emoji">{{ current.emoji }}</span>
      <b>{{ title || current.label }}:</b>
    </span>
    <div class="ud-tip-body">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ud-tip {
  background: var(--tip-bg);
  border-left: 4px solid var(--tip-color);
  border-radius: 0 var(--ud-radius) var(--ud-radius) 0;
  padding: 0.55em 1em;
  font-size: 0.92em;
  text-align: left;
}

.ud-tip-head {
  display: flex;
  align-items: center;
  gap: 0.45em;
  color: var(--tip-color);
  font-weight: 700;
  margin-bottom: 0.25em;
}

.ud-tip-emoji {
  font-size: 1.1em;
}

.ud-tip-body {
  color: var(--ud-text-mute);
}

.ud-tip-body :deep(p) {
  margin: 0.2em 0;
}
</style>
