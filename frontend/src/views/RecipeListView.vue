<script setup lang="ts">
import { onMounted } from 'vue'
import { useRecipeStore } from '@/stores/recipeStore'
import RecipeCard from '@/components/RecipeCard.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'

const recipeStore = useRecipeStore()

onMounted(async () => {
  await recipeStore.fetchRecipes()
})
</script>

<template>
  <div class="recipe-list-view">
    <header class="page-header">
      <h1>All Recipes</h1>
    </header>

    <LoadingSpinner v-if="recipeStore.isLoading" />

    <div v-else-if="recipeStore.error" class="error-message" role="alert">
      {{ recipeStore.error }}
      <button class="btn btn-secondary" @click="recipeStore.fetchRecipes">
        Try Again
      </button>
    </div>

    <EmptyState 
      v-else-if="recipeStore.recipes.length === 0"
      title="No recipes yet"
      message="Your recipe collection is empty."
    />

    <div v-else class="recipe-grid" role="list" aria-label="Recipe list">
      <RecipeCard
        v-for="recipe in recipeStore.recipes"
        :key="recipe.id"
        :recipe="recipe"
        :show-remove="false"
        :show-edit="true"
        role="listitem"
      />
    </div>
  </div>
</template>

<style scoped>
.recipe-list-view {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: var(--spacing-lg);
}

.page-header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0;
}

.recipe-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--spacing-md);
}

.error-message {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--color-error);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-md);
}

@media (max-width: 768px) {
  .recipe-grid {
    grid-template-columns: 1fr;
  }
}
</style>
