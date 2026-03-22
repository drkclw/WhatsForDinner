<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useRecipeStore } from '@/stores/recipeStore'
import RecipeCard from '@/components/RecipeCard.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import ErrorNotification from '@/components/ErrorNotification.vue'

const recipeStore = useRecipeStore()

const showDeleteDialog = ref(false)
const recipeToDelete = ref<{ id: number; name: string } | null>(null)
const isDeleting = ref(false)
const showSuccess = ref(false)
const successMessage = ref('')
const showError = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  await recipeStore.fetchRecipes()
})

function confirmDelete(recipe: { id: number; name: string }) {
  recipeToDelete.value = recipe
  showDeleteDialog.value = true
}

async function handleDeleteConfirm() {
  if (!recipeToDelete.value) return

  isDeleting.value = true
  const name = recipeToDelete.value.name
  const success = await recipeStore.deleteRecipe(recipeToDelete.value.id)

  isDeleting.value = false
  showDeleteDialog.value = false
  recipeToDelete.value = null

  if (success) {
    successMessage.value = `"${name}" has been deleted.`
    showSuccess.value = true
  } else {
    errorMessage.value = recipeStore.error || 'Failed to delete recipe'
    showError.value = true
  }
}

function handleDeleteCancel() {
  showDeleteDialog.value = false
  recipeToDelete.value = null
}
</script>

<template>
  <div class="recipe-list-view">
    <header class="page-header">
      <h1>All Recipes</h1>
      <RouterLink to="/recipes/new" class="btn btn-primary" aria-label="Add a new recipe">
        + Add Recipe
      </RouterLink>
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
      <div v-for="recipe in recipeStore.recipes" :key="recipe.id" class="recipe-card-wrapper" role="listitem">
        <RecipeCard
          :recipe="recipe"
          :show-remove="false"
          :show-edit="true"
        />
        <button
          class="btn btn-danger btn-delete"
          :aria-label="`Delete ${recipe.name}`"
          @click="confirmDelete({ id: recipe.id, name: recipe.name })"
        >
          Delete
        </button>
      </div>
    </div>

    <ConfirmDialog
      :show="showDeleteDialog"
      title="Delete Recipe"
      :message="`Are you sure you want to delete &quot;${recipeToDelete?.name ?? ''}&quot;? This will also remove it from the weekly plan.`"
      confirm-label="Delete"
      cancel-label="Cancel"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <ErrorNotification
      :message="successMessage"
      :show="showSuccess"
      type="success"
      :duration="3000"
      @close="showSuccess = false"
    />

    <ErrorNotification
      :message="errorMessage"
      :show="showError"
      type="error"
      @close="showError = false"
    />
  </div>
</template>

<style scoped>
.recipe-list-view {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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

.recipe-card-wrapper {
  display: flex;
  flex-direction: column;
}

.btn-delete {
  margin-top: var(--spacing-xs);
  align-self: flex-end;
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
