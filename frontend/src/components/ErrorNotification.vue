<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
  message: string;
  show: boolean;
  type?: 'error' | 'warning' | 'success' | 'info';
  duration?: number;
}>();

const emit = defineEmits<{
  close: [];
}>();

const visible = ref(props.show);

watch(() => props.show, (newValue) => {
  visible.value = newValue;
  if (newValue && props.duration && props.duration > 0) {
    setTimeout(() => {
      emit('close');
    }, props.duration);
  }
});

const handleClose = () => {
  emit('close');
};

const typeClasses = {
  error: 'notification--error',
  warning: 'notification--warning',
  success: 'notification--success',
  info: 'notification--info'
};
</script>

<template>
  <Teleport to="body">
    <Transition name="notification">
      <div 
        v-if="visible" 
        class="notification" 
        :class="typeClasses[type || 'error']"
        role="alert"
        aria-live="assertive"
        aria-atomic="true"
      >
        <div class="notification__content">
          <span class="notification__icon" aria-hidden="true">
            <template v-if="type === 'success'">✓</template>
            <template v-else-if="type === 'warning'">⚠</template>
            <template v-else-if="type === 'info'">ℹ</template>
            <template v-else>✕</template>
          </span>
          <p class="notification__message">{{ message }}</p>
        </div>
        <button 
          type="button"
          class="notification__close" 
          @click="handleClose"
          aria-label="Close notification"
        >
          ×
        </button>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.notification {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  min-width: 300px;
  max-width: 500px;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  animation: slideIn 0.3s ease-out;
}

.notification__content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.notification__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  font-size: 14px;
  font-weight: bold;
}

.notification__message {
  margin: 0;
  font-size: 0.875rem;
  line-height: 1.4;
}

.notification__close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  opacity: 0.7;
  transition: opacity 0.2s;
}

.notification__close:hover {
  opacity: 1;
}

/* Type variations */
.notification--error {
  background-color: #fee2e2;
  border-left: 4px solid #ef4444;
  color: #991b1b;
}

.notification--error .notification__icon {
  background-color: #ef4444;
  color: white;
}

.notification--error .notification__close {
  color: #991b1b;
}

.notification--warning {
  background-color: #fef3c7;
  border-left: 4px solid #f59e0b;
  color: #92400e;
}

.notification--warning .notification__icon {
  background-color: #f59e0b;
  color: white;
}

.notification--warning .notification__close {
  color: #92400e;
}

.notification--success {
  background-color: #d1fae5;
  border-left: 4px solid #10b981;
  color: #065f46;
}

.notification--success .notification__icon {
  background-color: #10b981;
  color: white;
}

.notification--success .notification__close {
  color: #065f46;
}

.notification--info {
  background-color: #dbeafe;
  border-left: 4px solid #3b82f6;
  color: #1e40af;
}

.notification--info .notification__icon {
  background-color: #3b82f6;
  color: white;
}

.notification--info .notification__close {
  color: #1e40af;
}

/* Transitions */
.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Responsive */
@media (max-width: 640px) {
  .notification {
    left: 1rem;
    right: 1rem;
    min-width: auto;
    max-width: none;
  }
}
</style>
