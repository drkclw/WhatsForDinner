<script setup lang="ts">
import type { Recipe } from '@/types/Recipe'
import { RouterLink } from 'vue-router'

interface Props {
  recipe: Recipe
  weeklyPlanItemId?: number
  showRemove?: boolean
  showEdit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showRemove: false,
  showEdit: true
})

const emit = defineEmits<{
  remove: [itemId: number]
}>()

function handleRemove() {
  if (props.weeklyPlanItemId) {
    emit('remove', props.weeklyPlanItemId)
  }
}

function formatCookTime(minutes: number | null): string {
  if (!minutes) return ''
  if (minutes < 60) return `${minutes} min`
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`
}
</script>

<template>
  <article class="recipe-card card">
    <header class="recipe-header">
      <h3 class="recipe-name">{{ recipe.name }}</h3>
      <span v-if="recipe.cookTimeMinutes" class="cook-time" aria-label="Cook time">
        🕐 {{ formatCookTime(recipe.cookTimeMinutes) }}
      </span>
    </header>
    
    <p v-if="recipe.description" class="recipe-description">
      {{ recipe.description }}
    </p>
    
    <footer class="recipe-actions">
      <RouterLink 
        v-if="showEdit"
        :to="`/recipes/${recipe.id}/edit`" 
        class="btn btn-secondary"
        :aria-label="`Edit ${recipe.name}`"
      >
        Edit
      </RouterLink>
      <button 
        v-if="showRemove"
        class="btn btn-danger"
        @click="handleRemove"
        :aria-label="`Remove ${recipe.name} from plan`"
      >
        Remove
      </button>
    </footer>
  </article>
</template>

<style scoped>
.recipe-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.recipe-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-sm);
}

.recipe-name {
  font-size: 1.125rem;
  font-weight: 600;
  margin: 0;
  color: var(--color-text);
}

.cook-time {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  white-space: nowrap;
}

.recipe-description {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  margin: 0;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.recipe-actions {
  display: flex;
  gap: var(--spacing-sm);
  margin-top: auto;
  padding-top: var(--spacing-sm);
}
</style>
