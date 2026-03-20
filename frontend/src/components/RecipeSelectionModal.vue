<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import type { Recipe } from '@/types/Recipe'
import RecipeListItem from '@/components/RecipeListItem.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

interface Props {
  recipes: Recipe[]
  isLoading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  select: [recipeId: number]
  close: []
}>()

const modalRef = ref<HTMLElement | null>(null)

function handleSelect(recipeId: number) {
  emit('select', recipeId)
}

function handleClose() {
  emit('close')
}

function handleBackdropClick(event: MouseEvent) {
  if (event.target === event.currentTarget) {
    handleClose()
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    handleClose()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
  document.body.style.overflow = 'hidden'
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
  document.body.style.overflow = ''
})
</script>

<template>
  <div 
    class="modal-backdrop" 
    @click="handleBackdropClick"
    role="dialog"
    aria-modal="true"
    aria-labelledby="modal-title"
  >
    <div class="modal" ref="modalRef">
      <header class="modal-header">
        <h2 id="modal-title">Select a Recipe</h2>
        <button 
          class="modal-close" 
          @click="handleClose"
          aria-label="Close dialog"
        >
          ✕
        </button>
      </header>
      
      <div class="modal-body">
        <LoadingSpinner v-if="isLoading" />
        
        <ul v-else-if="recipes.length" class="recipe-list" role="listbox">
          <RecipeListItem
            v-for="recipe in recipes"
            :key="recipe.id"
            :recipe="recipe"
            @select="handleSelect"
          />
        </ul>
        
        <p v-else class="no-recipes">No recipes available.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: var(--spacing-md);
}

.modal {
  background-color: var(--color-surface);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  max-width: 500px;
  width: 100%;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
}

.modal-header h2 {
  margin: 0;
  font-size: 1.25rem;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: var(--color-text-secondary);
  padding: var(--spacing-xs);
  cursor: pointer;
}

.modal-close:hover {
  color: var(--color-text);
}

.modal-body {
  padding: var(--spacing-md);
  overflow-y: auto;
}

.recipe-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.no-recipes {
  text-align: center;
  color: var(--color-text-secondary);
  padding: var(--spacing-lg);
}
</style>
