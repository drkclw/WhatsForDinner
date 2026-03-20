<script setup lang="ts">
import type { Recipe } from '@/types/Recipe'

interface Props {
  recipe: Recipe
}

const props = defineProps<Props>()

const emit = defineEmits<{
  select: [recipeId: number]
}>()

function handleSelect() {
  emit('select', props.recipe.id)
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
  <li 
    class="recipe-list-item" 
    role="option"
    tabindex="0"
    @click="handleSelect"
    @keydown.enter="handleSelect"
    @keydown.space.prevent="handleSelect"
  >
    <div class="recipe-info">
      <span class="recipe-name">{{ recipe.name }}</span>
      <span v-if="recipe.cookTimeMinutes" class="cook-time">
        🕐 {{ formatCookTime(recipe.cookTimeMinutes) }}
      </span>
    </div>
    <p v-if="recipe.description" class="recipe-description">
      {{ recipe.description }}
    </p>
  </li>
</template>

<style scoped>
.recipe-list-item {
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: background-color 0.2s, border-color 0.2s;
}

.recipe-list-item:hover,
.recipe-list-item:focus {
  background-color: var(--color-primary-light);
  border-color: var(--color-primary);
  outline: none;
}

.recipe-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-sm);
}

.recipe-name {
  font-weight: 500;
  color: var(--color-text);
}

.cook-time {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.recipe-description {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  margin: var(--spacing-xs) 0 0;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
