<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import type { RecipeCreateRequest } from '@/types/Recipe'

interface Props {
  initialData?: Partial<RecipeCreateRequest> | null
  submitLabel?: string
  source?: 'manual' | 'extracted'
  isSubmitting?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  initialData: null,
  submitLabel: 'Save Recipe',
  source: 'manual',
  isSubmitting: false
})

const emit = defineEmits<{
  submit: [data: RecipeCreateRequest]
  cancel: []
}>()

const form = ref<RecipeCreateRequest>({
  name: props.initialData?.name ?? '',
  description: props.initialData?.description ?? null,
  ingredients: props.initialData?.ingredients ?? null,
  cookTimeMinutes: props.initialData?.cookTimeMinutes ?? null
})

const validationErrors = ref<Record<string, string>>({})

// Track which fields were populated by AI extraction
const extractedFields = reactive<Record<string, boolean>>({
  name: false,
  description: false,
  ingredients: false,
  cookTimeMinutes: false
})

watch(() => props.initialData, (newData) => {
  if (newData) {
    form.value = {
      name: newData.name ?? '',
      description: newData.description ?? null,
      ingredients: newData.ingredients ?? null,
      cookTimeMinutes: newData.cookTimeMinutes ?? null
    }
    // Mark extracted fields when source is 'extracted'
    if (props.source === 'extracted') {
      extractedFields.name = !!newData.name
      extractedFields.description = !!newData.description
      extractedFields.ingredients = !!newData.ingredients
      extractedFields.cookTimeMinutes = newData.cookTimeMinutes != null
    }
  }
}, { deep: true })

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

  if (form.value.cookTimeMinutes !== null && form.value.cookTimeMinutes !== undefined && form.value.cookTimeMinutes < 0) {
    validationErrors.value.cookTimeMinutes = 'Cook time must be 0 or greater'
  }

  return Object.keys(validationErrors.value).length === 0
}

function handleSubmit() {
  if (!validateForm()) {
    return
  }

  emit('submit', {
    name: form.value.name.trim(),
    description: form.value.description || null,
    ingredients: form.value.ingredients || null,
    cookTimeMinutes: form.value.cookTimeMinutes ?? null
  })
}

function handleCancel() {
  emit('cancel')
}
</script>

<template>
  <form class="recipe-form card" @submit.prevent="handleSubmit" novalidate>
    <div v-if="source === 'extracted'" class="extraction-notice" role="status">
      <span aria-hidden="true">✨</span>
      Review the extracted recipe details below and make any corrections before saving.
    </div>

    <div class="form-group">
      <label for="name" class="form-label">
        Name *
        <span v-if="extractedFields.name" class="ai-badge" aria-label="AI extracted">AI</span>
      </label>
      <input
        id="name"
        v-model="form.name"
        type="text"
        class="form-input"
        :class="{ 'input-error': validationErrors.name, 'ai-populated': extractedFields.name }"
        required
        maxlength="200"
        placeholder="Enter recipe name"
        :aria-invalid="!!validationErrors.name"
        :aria-describedby="validationErrors.name ? 'name-error' : undefined"
      />
      <span v-if="validationErrors.name" id="name-error" class="field-error" role="alert">
        {{ validationErrors.name }}
      </span>
    </div>

    <div class="form-group">
      <label for="description" class="form-label">
        Description
        <span v-if="extractedFields.description" class="ai-badge" aria-label="AI extracted">AI</span>
      </label>
      <textarea
        id="description"
        v-model="form.description"
        class="form-input form-textarea"
        :class="{ 'input-error': validationErrors.description, 'ai-populated': extractedFields.description }"
        maxlength="1000"
        rows="3"
        placeholder="Brief description of the dish"
        :aria-invalid="!!validationErrors.description"
        :aria-describedby="validationErrors.description ? 'description-error' : undefined"
      ></textarea>
      <span v-if="validationErrors.description" id="description-error" class="field-error" role="alert">
        {{ validationErrors.description }}
      </span>
    </div>

    <div class="form-group">
      <label for="ingredients" class="form-label">
        Ingredients (one per line)
        <span v-if="extractedFields.ingredients" class="ai-badge" aria-label="AI extracted">AI</span>
      </label>
      <textarea
        id="ingredients"
        v-model="form.ingredients"
        class="form-input form-textarea"
        :class="{ 'input-error': validationErrors.ingredients, 'ai-populated': extractedFields.ingredients }"
        maxlength="2000"
        rows="6"
        placeholder="Enter each ingredient on a new line"
        :aria-invalid="!!validationErrors.ingredients"
        :aria-describedby="validationErrors.ingredients ? 'ingredients-error' : undefined"
      ></textarea>
      <span v-if="validationErrors.ingredients" id="ingredients-error" class="field-error" role="alert">
        {{ validationErrors.ingredients }}
      </span>
    </div>

    <div class="form-group">
      <label for="cookTime" class="form-label">
        Cook Time (minutes)
        <span v-if="extractedFields.cookTimeMinutes" class="ai-badge" aria-label="AI extracted">AI</span>
      </label>
      <input
        id="cookTime"
        v-model.number="form.cookTimeMinutes"
        type="number"
        class="form-input form-input-small"
        :class="{ 'input-error': validationErrors.cookTimeMinutes, 'ai-populated': extractedFields.cookTimeMinutes }"
        min="0"
        placeholder="0"
        :aria-invalid="!!validationErrors.cookTimeMinutes"
        :aria-describedby="validationErrors.cookTimeMinutes ? 'cooktime-error' : undefined"
      />
      <span v-if="validationErrors.cookTimeMinutes" id="cooktime-error" class="field-error" role="alert">
        {{ validationErrors.cookTimeMinutes }}
      </span>
    </div>

    <div class="form-actions">
      <button
        type="button"
        class="btn btn-secondary"
        data-testid="cancel-btn"
        @click="handleCancel"
        :disabled="isSubmitting"
      >
        Cancel
      </button>
      <button
        type="submit"
        class="btn btn-primary"
        :disabled="isSubmitting"
      >
        {{ isSubmitting ? 'Saving...' : submitLabel }}
      </button>
    </div>
  </form>
</template>

<style scoped>
.recipe-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.extraction-notice {
  background-color: #E8F5E9;
  color: #2E7D32;
  padding: var(--spacing-md);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  line-height: 1.5;
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

.ai-badge {
  display: inline-block;
  background-color: #E3F2FD;
  color: #1565C0;
  font-size: 0.625rem;
  font-weight: 700;
  padding: 1px 5px;
  border-radius: 3px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  vertical-align: middle;
  margin-left: var(--spacing-xs);
}

.ai-populated {
  border-color: #90CAF9;
  background-color: #FAFCFF;
}

.ai-populated:focus {
  border-color: var(--color-primary);
  background-color: #fff;
}
</style>
