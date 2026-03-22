<script setup lang="ts">
import { ref, onUnmounted } from 'vue'

interface Props {
  isLoading?: boolean
}

withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  'file-selected': [file: File]
}>()

const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10 MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

const previewUrl = ref<string | null>(null)
const errorMessage = ref<string | null>(null)
const selectedFileName = ref<string | null>(null)
const isDragOver = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)

function validateFile(file: File): string | null {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Unsupported file format. Please upload a JPEG, PNG, or WebP image.'
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'File is too large. Maximum size is 10 MB.'
  }
  return null
}

function handleFile(file: File) {
  errorMessage.value = null
  cleanupPreview()

  const validationError = validateFile(file)
  if (validationError) {
    errorMessage.value = validationError
    return
  }

  selectedFileName.value = file.name
  previewUrl.value = URL.createObjectURL(file)
  emit('file-selected', file)
}

function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    handleFile(file)
  }
}

function handleDragOver(event: DragEvent) {
  event.preventDefault()
  isDragOver.value = true
}

function handleDragLeave() {
  isDragOver.value = false
}

function handleDrop(event: DragEvent) {
  event.preventDefault()
  isDragOver.value = false
  const file = event.dataTransfer?.files[0]
  if (file) {
    handleFile(file)
  }
}

function triggerFileInput() {
  fileInputRef.value?.click()
}

function cleanupPreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = null
  }
  selectedFileName.value = null
}

onUnmounted(() => {
  cleanupPreview()
})
</script>

<template>
  <div class="image-upload">
    <div
      class="upload-area"
      :class="{ 'drag-over': isDragOver, 'has-error': errorMessage }"
      @dragover="handleDragOver"
      @dragleave="handleDragLeave"
      @drop="handleDrop"
      @click="triggerFileInput"
      role="button"
      tabindex="0"
      @keydown.enter="triggerFileInput"
      @keydown.space.prevent="triggerFileInput"
      :aria-label="isLoading ? 'Processing image...' : 'Upload a recipe image'"
    >
      <input
        ref="fileInputRef"
        type="file"
        accept="image/jpeg,image/png,image/webp"
        class="file-input"
        aria-label="Upload a recipe image (JPEG, PNG, or WebP, max 10 MB)"
        @change="handleFileChange"
      />

      <div v-if="isLoading" class="upload-loading">
        <div class="spinner" aria-hidden="true"></div>
        <span aria-live="polite">Extracting recipe from image...</span>
      </div>

      <div v-else-if="previewUrl" class="upload-preview">
        <img :src="previewUrl" :alt="selectedFileName || 'Uploaded recipe image'" class="preview-image" />
        <p class="preview-filename">{{ selectedFileName }}</p>
      </div>

      <div v-else class="upload-prompt">
        <span class="upload-icon" aria-hidden="true">📷</span>
        <p class="upload-text">
          <strong>Click to upload</strong> or drag and drop
        </p>
        <p class="upload-hint">JPEG, PNG, or WebP (max 10 MB)</p>
      </div>
    </div>

    <div
      v-if="errorMessage"
      class="upload-error"
      role="alert"
      aria-live="assertive"
      :id="'upload-error'"
    >
      {{ errorMessage }}
    </div>
  </div>
</template>

<style scoped>
.image-upload {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.upload-area {
  border: 2px dashed var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s, background-color 0.2s;
  min-height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.upload-area:hover,
.upload-area:focus-visible {
  border-color: var(--color-primary);
  background-color: var(--color-primary-light);
}

.upload-area.drag-over {
  border-color: var(--color-primary);
  background-color: var(--color-primary-light);
}

.upload-area.has-error {
  border-color: var(--color-error);
}

.file-input {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.upload-prompt {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-xs);
}

.upload-icon {
  font-size: 2.5rem;
}

.upload-text {
  color: var(--color-text);
  margin: 0;
}

.upload-hint {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  margin: 0;
}

.upload-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
}

.preview-image {
  max-width: 200px;
  max-height: 200px;
  border-radius: var(--radius-sm);
  object-fit: cover;
}

.preview-filename {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  margin: 0;
}

.upload-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
  color: var(--color-text-secondary);
}

.upload-loading .spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.upload-error {
  color: var(--color-error);
  font-size: 0.875rem;
  padding: var(--spacing-xs) var(--spacing-sm);
}
</style>
