<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useRecipeStore } from '@/stores/recipeStore'
import type { RecipeCreateRequest } from '@/types/Recipe'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import RecipeForm from '@/components/RecipeForm.vue'
import ErrorNotification from '@/components/ErrorNotification.vue'

const route = useRoute()
const router = useRouter()
const recipeStore = useRecipeStore()

const recipeId = computed(() => Number(route.params.id))

const isSaving = ref(false)
const saveError = ref<string | null>(null)

const initialFormData = computed<Partial<RecipeCreateRequest> | null>(() => {
  if (!recipeStore.currentRecipe) return null
  return {
    name: recipeStore.currentRecipe.name,
    description: recipeStore.currentRecipe.description,
    ingredients: recipeStore.currentRecipe.ingredients,
    cookTimeMinutes: recipeStore.currentRecipe.cookTimeMinutes
  }
})

onMounted(async () => {
  await recipeStore.fetchRecipeById(recipeId.value)
})

async function handleSubmit(data: RecipeCreateRequest) {
  isSaving.value = true
  saveError.value = null

  const result = await recipeStore.updateRecipe(recipeId.value, {
    name: data.name,
    description: data.description,
    ingredients: data.ingredients,
    cookTimeMinutes: data.cookTimeMinutes
  })

  isSaving.value = false

  if (result) {
    router.push('/recipes')
  } else {
    saveError.value = recipeStore.error || 'Failed to save recipe'
  }
}

function handleCancel() {
  router.push('/recipes')
}
</script>

<template>
  <div class="recipe-edit-view">
    <header class="page-header">
      <h1>Edit Recipe</h1>
    </header>

    <LoadingSpinner v-if="recipeStore.isLoading" />

    <div v-else-if="recipeStore.error && !recipeStore.currentRecipe" class="error-message" role="alert">
      {{ recipeStore.error }}
      <button class="btn btn-secondary" @click="router.push('/recipes')">
        Back to Recipes
      </button>
    </div>

    <template v-else-if="recipeStore.currentRecipe">
      <ErrorNotification
        :message="saveError ?? ''"
        :show="!!saveError"
        type="error"
        @close="saveError = null"
      />

      <RecipeForm
        :initial-data="initialFormData"
        submit-label="Save Changes"
        source="manual"
        :is-submitting="isSaving"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </template>
  </div>
</template>

<style scoped>
.recipe-edit-view {
  max-width: 600px;
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

.error-message {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--color-error);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-md);
}
</style>
