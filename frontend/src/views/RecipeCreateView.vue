<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useRecipeStore } from '@/stores/recipeStore'
import { recipeService } from '@/services/recipeService'
import type { RecipeCreateRequest, RecipeImageExtractResult } from '@/types/Recipe'
import RecipeForm from '@/components/RecipeForm.vue'
import ImageUpload from '@/components/ImageUpload.vue'
import ErrorNotification from '@/components/ErrorNotification.vue'

const router = useRouter()
const recipeStore = useRecipeStore()

const isSaving = ref(false)
const isExtracting = ref(false)
const showSuccess = ref(false)
const showError = ref(false)
const errorMessage = ref('')
const activeTab = ref<'manual' | 'image'>('manual')
const initialData = ref<Partial<RecipeCreateRequest> | null>(null)
const formSource = ref<'manual' | 'extracted'>('manual')
const extractionMessage = ref<string | null>(null)

async function handleFileSelected(file: File) {
  isExtracting.value = true
  showError.value = false
  extractionMessage.value = null

  try {
    const result: RecipeImageExtractResult = await recipeService.extractFromImage(file)

    if (result.success) {
      const extractedFieldNames: string[] = []
      const emptyFieldNames: string[] = []

      if (result.name) extractedFieldNames.push('Name')
      else emptyFieldNames.push('Name')

      if (result.description) extractedFieldNames.push('Description')
      else emptyFieldNames.push('Description')

      if (result.ingredients) extractedFieldNames.push('Ingredients')
      else emptyFieldNames.push('Ingredients')

      if (result.cookTimeMinutes != null) extractedFieldNames.push('Cook Time')
      else emptyFieldNames.push('Cook Time')

      initialData.value = {
        name: result.name ?? '',
        description: result.description ?? null,
        ingredients: result.ingredients ?? null,
        cookTimeMinutes: result.cookTimeMinutes ?? null
      }
      formSource.value = 'extracted'

      let statusMsg = 'Recipe data extracted successfully.'
      if (extractedFieldNames.length > 0) {
        statusMsg += ` Extracted: ${extractedFieldNames.join(', ')}.`
      }
      if (emptyFieldNames.length > 0) {
        statusMsg += ` Not detected: ${emptyFieldNames.join(', ')} — please fill in manually.`
      }
      statusMsg += ' Review and edit before saving.'
      extractionMessage.value = statusMsg
      activeTab.value = 'manual' // Switch to form to review
    } else {
      errorMessage.value = result.message || 'Could not extract recipe from image'
      showError.value = true
    }
  } catch (e) {
    errorMessage.value = e instanceof Error ? e.message : 'Failed to extract recipe from image'
    showError.value = true
  } finally {
    isExtracting.value = false
  }
}

async function handleSubmit(data: RecipeCreateRequest) {
  isSaving.value = true
  showError.value = false

  const result = await recipeStore.createRecipe(data)

  isSaving.value = false

  if (result) {
    showSuccess.value = true
    setTimeout(() => {
      router.push('/recipes')
    }, 1000)
  } else {
    errorMessage.value = recipeStore.error || 'Failed to create recipe'
    showError.value = true
  }
}

function handleCancel() {
  router.back()
}

function switchToManual() {
  activeTab.value = 'manual'
  formSource.value = 'manual'
  initialData.value = null
  extractionMessage.value = null
}
</script>

<template>
  <div class="recipe-create-view">
    <header class="page-header">
      <h1>Add New Recipe</h1>
    </header>

    <div class="tab-bar" role="tablist" aria-label="Recipe entry method">
      <button
        role="tab"
        :aria-selected="activeTab === 'manual'"
        class="tab-btn"
        :class="{ active: activeTab === 'manual' }"
        @click="switchToManual"
      >
        ✏️ Manual Entry
      </button>
      <button
        role="tab"
        :aria-selected="activeTab === 'image'"
        class="tab-btn"
        :class="{ active: activeTab === 'image' }"
        @click="activeTab = 'image'"
      >
        📷 Upload Image
      </button>
    </div>

    <div v-if="extractionMessage" class="extraction-status" role="status">
      {{ extractionMessage }}
    </div>

    <div v-if="activeTab === 'image'" class="image-upload-section" role="tabpanel">
      <ImageUpload
        :is-loading="isExtracting"
        @file-selected="handleFileSelected"
      />
      <div v-if="showError && activeTab === 'image'" class="error-fallback">
        <p>{{ errorMessage }}</p>
        <div class="fallback-actions">
          <button class="btn btn-secondary" @click="activeTab = 'image'">Try Another Image</button>
          <button class="btn btn-primary" @click="switchToManual">Enter Manually</button>
        </div>
      </div>
    </div>

    <div v-if="activeTab === 'manual'" role="tabpanel">
      <RecipeForm
        :initial-data="initialData"
        :source="formSource"
        submit-label="Create Recipe"
        :is-submitting="isSaving"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>

    <ErrorNotification
      :message="errorMessage"
      :show="showError && activeTab === 'manual'"
      type="error"
      @close="showError = false"
    />

    <ErrorNotification
      message="Recipe created successfully!"
      :show="showSuccess"
      type="success"
      :duration="2000"
      @close="showSuccess = false"
    />
  </div>
</template>

<style scoped>
.recipe-create-view {
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

.tab-bar {
  display: flex;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-md);
  border-bottom: 2px solid var(--color-border);
}

.tab-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  font-size: 1rem;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: color 0.2s, border-color 0.2s;
}

.tab-btn:hover {
  color: var(--color-text);
}

.tab-btn.active {
  color: var(--color-primary);
  border-bottom-color: var(--color-primary);
  font-weight: 500;
}

.extraction-status {
  background-color: #E8F5E9;
  color: #2E7D32;
  padding: var(--spacing-md);
  border-radius: var(--radius-sm);
  margin-bottom: var(--spacing-md);
  font-size: 0.875rem;
}

.image-upload-section {
  margin-bottom: var(--spacing-md);
}

.error-fallback {
  text-align: center;
  padding: var(--spacing-md);
  color: var(--color-error);
}

.fallback-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: center;
  margin-top: var(--spacing-md);
}
</style>
