<script setup lang="ts">
interface Props {
  title: string
  message: string
  actionLabel?: string
}

defineProps<Props>()

const emit = defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="empty-state" role="status">
    <div class="empty-state-icon" aria-hidden="true">📋</div>
    <h2 class="empty-state-title">{{ title }}</h2>
    <p class="empty-state-message">{{ message }}</p>
    <button 
      v-if="actionLabel" 
      class="btn btn-primary"
      @click="emit('action')"
    >
      {{ actionLabel }}
    </button>
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-xl);
  text-align: center;
  min-height: 300px;
}

.empty-state-icon {
  font-size: 4rem;
  margin-bottom: var(--spacing-md);
}

.empty-state-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 var(--spacing-sm);
}

.empty-state-message {
  color: var(--color-text-secondary);
  margin: 0 0 var(--spacing-lg);
  max-width: 400px;
}
</style>
