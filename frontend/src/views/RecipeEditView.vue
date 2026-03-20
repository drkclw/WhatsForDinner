<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useRecipeStore } from '@/stores/recipeStore'
import type { RecipeUpdateRequest } from '@/types/Recipe'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const route = useRoute()
const router = useRouter()
const recipeStore = useRecipeStore()

const recipeId = computed(() => Number(route.params.id))

const form = ref<RecipeUpdateRequest>({
  name: '',
  description: null,
  ingredients: null,
  cookTimeMinutes: null
})

const isSaving = ref(false)
const saveError = ref<string | null>(null)
const validationErrors = ref<Record<string, string>>({})

onMounted(async () => {
  await recipeStore.fetchRecipeById(recipeId.value)
  if (recipeStore.currentRecipe) {
    form.value = {
      name: recipeStore.currentRecipe.name,
      description: recipeStore.currentRecipe.description,
      ingredients: recipeStore.currentRecipe.ingredients,
      cookTimeMinutes: recipeStore.currentRecipe.cookTimeMinutes
    }
  }
})

watch(() => recipeStore.currentRecipe, (recipe) => {
  if (recipe) {
    form.value = {
      name: recipe.name,
      description: recipe.description,
      ingredients: recipe.ingredients,
      cookTimeMinutes: recipe.cookTimeMinutes
    }
  }
})

function validateForm(): boolean {
  validationErrors.value = {}
  
  if (!form.value.name || form.value.name.trim().length === 0) {
    validationErrors.value.name = 'Name is required'
  } else if (form.value.name.length > 200) {
    validationErrors.value.name = 'Name must be 200 characters or less'
  }
  
  if (form.value.description && form.value.description.length > 1000) {
    validationErrors.value.description = 'Description must be 1000 characters or less'
  }
  
  if (form.value.ingredients && form.value.ingredients.length > 2000) {
    validationErrors.value.ingredients = 'Ingredients must be 2000 characters or less'
  }
  
  if (form.value.cookTimeMinutes !== null && form.value.cookTimeMinutes < 0) {
    validationErrors.value.cookTimeMinutes = 'Cook time must be 0 or greater'
  }
  
  return Object.keys(validationErrors.value).length === 0
}

async function handleSubmit() {
  if (!validateForm()) {
    return
  }
  
  isSaving.value = true
  saveError.value = null
  
  const result = await recipeStore.updateRecipe(recipeId.value, form.value)
  
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

    <form v-else-if="recipeStore.currentRecipe" class="edit-form card" @submit.prevent="handleSubmit">
      <div v-if="saveError" class="form-error" role="alert">
        {{ saveError }}
      </div>

      <div class="form-group">
        <label for="name" class="form-label">Name *</label>
        <input
          id="name"
          v-model="form.name"
          type="text"
          class="form-input"
          :class="{ 'input-error': validationErrors.name }"
          required
          maxlength="200"
          :aria-invalid="!!validationErrors.name"
          :aria-describedby="validationErrors.name ? 'name-error' : undefined"
        />
        <span v-if="validationErrors.name" id="name-error" class="field-error">
          {{ validationErrors.name }}
        </span>
      </div>

      <div class="form-group">
        <label for="description" class="form-label">Description</label>
        <textarea
          id="description"
          v-model="form.description"
          class="form-input form-textarea"
          :class="{ 'input-error': validationErrors.description }"
          maxlength="1000"
          rows="3"
          :aria-invalid="!!validationErrors.description"
          :aria-describedby="validationErrors.description ? 'description-error' : undefined"
        ></textarea>
        <span v-if="validationErrors.description" id="description-error" class="field-error">
          {{ validationErrors.description }}
        </span>
      </div>

      <div class="form-group">
        <label for="ingredients" class="form-label">Ingredients (one per line)</label>
        <textarea
          id="ingredients"
          v-model="form.ingredients"
          class="form-input form-textarea"
          :class="{ 'input-error': validationErrors.ingredients }"
          maxlength="2000"
          rows="6"
          :aria-invalid="!!validationErrors.ingredients"
          :aria-describedby="validationErrors.ingredients ? 'ingredients-error' : undefined"
        ></textarea>
        <span v-if="validationErrors.ingredients" id="ingredients-error" class="field-error">
          {{ validationErrors.ingredients }}
        </span>
      </div>

      <div class="form-group">
        <label for="cookTime" class="form-label">Cook Time (minutes)</label>
        <input
          id="cookTime"
          v-model.number="form.cookTimeMinutes"
          type="number"
          class="form-input form-input-small"
          :class="{ 'input-error': validationErrors.cookTimeMinutes }"
          min="0"
          :aria-invalid="!!validationErrors.cookTimeMinutes"
          :aria-describedby="validationErrors.cookTimeMinutes ? 'cooktime-error' : undefined"
        />
        <span v-if="validationErrors.cookTimeMinutes" id="cooktime-error" class="field-error">
          {{ validationErrors.cookTimeMinutes }}
        </span>
      </div>

      <div class="form-actions">
        <button 
          type="button" 
          class="btn btn-secondary"
          @click="handleCancel"
          :disabled="isSaving"
        >
          Cancel
        </button>
        <button 
          type="submit" 
          class="btn btn-primary"
          :disabled="isSaving"
        >
          {{ isSaving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>
    </form>
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

.edit-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.form-error {
  background-color: #FFEBEE;
  color: var(--color-error);
  padding: var(--spacing-md);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.form-label {
  font-weight: 500;
  color: var(--color-text);
}

.form-input {
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: 1rem;
  font-family: inherit;
  transition: border-color 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-textarea {
  resize: vertical;
  min-height: 80px;
}

.form-input-small {
  max-width: 150px;
}

.input-error {
  border-color: var(--color-error);
}

.field-error {
  color: var(--color-error);
  font-size: 0.875rem;
}

.form-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
  margin-top: var(--spacing-md);
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
