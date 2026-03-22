<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'

interface Props {
  show: boolean
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
}

const props = withDefaults(defineProps<Props>(), {
  confirmLabel: 'Confirm',
  cancelLabel: 'Cancel'
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

const dialogRef = ref<HTMLElement | null>(null)

watch(() => props.show, async (visible) => {
  if (visible) {
    await nextTick()
    dialogRef.value?.focus()
  }
})

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    emit('cancel')
  } else if (e.key === 'Enter') {
    emit('confirm')
  }
}
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="dialog-overlay" @click.self="emit('cancel')">
      <div
        ref="dialogRef"
        role="dialog"
        aria-modal="true"
        :aria-label="title"
        class="dialog-content card"
        tabindex="-1"
        @keydown="handleKeydown"
      >
        <h2 class="dialog-title">{{ title }}</h2>
        <p class="dialog-message">{{ message }}</p>
        <div class="dialog-actions">
          <button
            data-testid="cancel-btn"
            class="btn btn-secondary"
            @click="emit('cancel')"
          >
            {{ cancelLabel }}
          </button>
          <button
            data-testid="confirm-btn"
            class="btn btn-danger"
            @click="emit('confirm')"
          >
            {{ confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.dialog-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-content {
  max-width: 420px;
  width: 90%;
  padding: var(--spacing-lg);
  outline: none;
}

.dialog-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 var(--spacing-sm) 0;
}

.dialog-message {
  color: var(--color-text-secondary);
  font-size: 0.9375rem;
  line-height: 1.5;
  margin: 0 0 var(--spacing-lg) 0;
}

.dialog-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
}
</style>
