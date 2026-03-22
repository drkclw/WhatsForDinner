<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { useWeeklyPlanStore } from '@/stores/weeklyPlanStore'
import { useRecipeStore } from '@/stores/recipeStore'
import RecipeCard from '@/components/RecipeCard.vue'
import EmptyState from '@/components/EmptyState.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import RecipeSelectionModal from '@/components/RecipeSelectionModal.vue'

const weeklyPlanStore = useWeeklyPlanStore()
const recipeStore = useRecipeStore()
const showAddModal = ref(false)

onMounted(async () => {
  await Promise.all([
    weeklyPlanStore.fetchWeeklyPlan(),
    recipeStore.fetchRecipes()
  ])
})

function openAddModal() {
  showAddModal.value = true
}

function closeAddModal() {
  showAddModal.value = false
}

async function handleAddRecipe(recipeId: number) {
  await weeklyPlanStore.addRecipe(recipeId)
  closeAddModal()
}

async function handleRemoveRecipe(itemId: number) {
  await weeklyPlanStore.removeRecipe(itemId)
}
</script>

<template>
  <div class="weekly-plan-view">
    <header class="page-header">
      <h1>Weekly Meal Plan</h1>
      <div class="header-actions">
        <RouterLink to="/recipes/new" class="btn btn-secondary" aria-label="Add a new recipe">
          + Add Recipe
        </RouterLink>
        <button 
          class="btn btn-primary"
          @click="openAddModal"
          :disabled="weeklyPlanStore.isLoading"
          aria-label="Add recipe to weekly plan"
        >
          + Add to Plan
        </button>
      </div>
    </header>

    <LoadingSpinner v-if="weeklyPlanStore.isLoading" />

    <div v-else-if="weeklyPlanStore.error" class="error-message" role="alert">
      {{ weeklyPlanStore.error }}
      <button class="btn btn-secondary" @click="weeklyPlanStore.fetchWeeklyPlan">
        Try Again
      </button>
    </div>

    <EmptyState 
      v-else-if="weeklyPlanStore.isEmpty"
      title="No recipes planned"
      message="Add some recipes to start planning your week!"
      :action-label="'Add Your First Recipe'"
      @action="openAddModal"
    />

    <div v-else class="recipe-grid" role="list" aria-label="Planned recipes">
      <RecipeCard
        v-for="item in weeklyPlanStore.items"
        :key="item.id"
        :recipe="item.recipe"
        :weekly-plan-item-id="item.id"
        :show-remove="true"
        @remove="handleRemoveRecipe"
        role="listitem"
      />
    </div>

    <RecipeSelectionModal
      v-if="showAddModal"
      :recipes="recipeStore.recipes"
      :is-loading="recipeStore.isLoading"
      @select="handleAddRecipe"
      @close="closeAddModal"
    />
  </div>
</template>

<style scoped>
.weekly-plan-view {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
}

.header-actions {
  display: flex;
  gap: var(--spacing-sm);
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
  .page-header {
    flex-direction: column;
    gap: var(--spacing-sm);
    align-items: stretch;
  }
  
  .recipe-grid {
    grid-template-columns: 1fr;
  }
}
</style>
