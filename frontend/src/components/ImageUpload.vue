<script setup lang="ts">
import { ref, onUnmounted } from 'vue'

interface Props {
  isLoading?: boolean
  loadingMessage?: string
}

const {
  isLoading = false,
  loadingMessage = 'Extracting recipe from image...'
} = defineProps<Props>()

const emit = defineEmits<{
  'files-changed': [files: File[]]
  'extract': []
}>()

const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10 MB
const MAX_FILE_COUNT = 5
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

const maxFileSize = MAX_FILE_SIZE
const maxFileCount = MAX_FILE_COUNT
const allowedTypes = ALLOWED_TYPES

interface UploadedFile {
  file: File
  previewUrl: string
}

const uploadedFiles = ref<UploadedFile[]>([])
const errorMessage = ref<string | null>(null)
const isDragOver = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)

function validateFile(file: File): string | null {
  if (!allowedTypes.includes(file.type)) {
    return `"${file.name}" is not a supported format. Please upload JPEG, PNG, or WebP images.`
  }
  if (file.size > maxFileSize) {
    return `"${file.name}" is too large. Maximum size is 10 MB per image.`
  }
  return null
}

function addFiles(newFiles: FileList | File[]) {
  errorMessage.value = null

  const filesToAdd = Array.from(newFiles)
  const remainingSlots = maxFileCount - uploadedFiles.value.length

  if (filesToAdd.length > remainingSlots) {
    if (remainingSlots === 0) {
      errorMessage.value = `Maximum of ${maxFileCount} images allowed. Remove an image to add more.`
      return
    }
    errorMessage.value = `Only ${remainingSlots} more image${remainingSlots === 1 ? '' : 's'} can be added (max ${maxFileCount}).`
    return
  }

  for (const file of filesToAdd) {
    const validationError = validateFile(file)
    if (validationError) {
      errorMessage.value = validationError
      return
    }
  }

  for (const file of filesToAdd) {
    uploadedFiles.value.push({
      file,
      previewUrl: URL.createObjectURL(file)
    })
  }

  emitFilesChanged()
}

function removeFile(index: number) {
  const removed = uploadedFiles.value.splice(index, 1)
  if (removed[0]) {
    URL.revokeObjectURL(removed[0].previewUrl)
  }
  errorMessage.value = null
  emitFilesChanged()
}

function emitFilesChanged() {
  emit('files-changed', uploadedFiles.value.map(u => u.file))
}

function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  if (input.files && input.files.length > 0) {
    addFiles(input.files)
  }
  // Reset input so the same file can be re-selected
  input.value = ''
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
  if (event.dataTransfer?.files) {
    addFiles(event.dataTransfer.files)
  }
}

function triggerFileInput() {
  fileInputRef.value?.click()
}

function handleExtract() {
  emit('extract')
}

function cleanupAllPreviews() {
  for (const uploaded of uploadedFiles.value) {
    URL.revokeObjectURL(uploaded.previewUrl)
  }
  uploadedFiles.value = []
}

onUnmounted(() => {
  cleanupAllPreviews()
})
</script>

<template>
  <div class="image-upload">
    <!-- Loading state -->
    <div v-if="isLoading" class="upload-loading" role="status">
      <div class="spinner" aria-hidden="true"></div>
      <span aria-live="polite">{{ loadingMessage }}</span>
    </div>

    <template v-else>
      <!-- Thumbnail grid when files are selected -->
      <div v-if="uploadedFiles.length > 0" class="thumbnail-section">
        <div class="thumbnail-grid" role="list" aria-label="Uploaded images">
          <div
            v-for="(uploaded, index) in uploadedFiles"
            :key="index"
            class="thumbnail-item"
            role="listitem"
          >
            <img
              :src="uploaded.previewUrl"
              :alt="uploaded.file.name"
              class="thumbnail-image"
            />
            <button
              type="button"
              class="thumbnail-remove"
              :aria-label="`Remove image ${index + 1}`"
              @click="removeFile(index)"
            >
              ✕
            </button>
            <p class="thumbnail-name">{{ uploaded.file.name }}</p>
          </div>
        </div>

        <div class="upload-actions">
          <button
            v-if="uploadedFiles.length < maxFileCount"
            type="button"
            class="btn btn-secondary btn-sm"
            @click="triggerFileInput"
          >
            + Add More
          </button>
          <button
            type="button"
            class="btn btn-primary"
            @click="handleExtract"
          >
            Extract Recipe
          </button>
        </div>
      </div>

      <!-- Empty state: upload prompt -->
      <div
        v-else
        class="upload-area"
        :class="{ 'drag-over': isDragOver, 'has-error': !!errorMessage }"
        @dragover="handleDragOver"
        @dragleave="handleDragLeave"
        @drop="handleDrop"
        @click="triggerFileInput"
        role="button"
        tabindex="0"
        @keydown.enter="triggerFileInput"
        @keydown.space.prevent="triggerFileInput"
        aria-label="Upload recipe images"
      >
        <div class="upload-prompt">
          <span class="upload-icon" aria-hidden="true">📷</span>
          <p class="upload-text">
            <strong>Click to upload</strong> or drag and drop
          </p>
          <p class="upload-hint">JPEG, PNG, or WebP (max ${maxFileSize / (1024 * 1024)} MB each, up to ${maxFileCount} images)</p>
        </div>
      </div>
    </template>

    <input
      ref="fileInputRef"
      type="file"
      accept="image/jpeg,image/png,image/webp"
      multiple
      class="file-input"
      aria-label="Upload recipe images (JPEG, PNG, or WebP, max ${maxFileSize / (1024 * 1024)} MB each, up to ${maxFileCount} images)"
      @change="handleFileChange"
    />

    <div
      v-if="errorMessage"
      class="upload-error"
      role="alert"
      aria-live="assertive"
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

.thumbnail-section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.thumbnail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: var(--spacing-sm);
}

.thumbnail-item {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-xs);
}

.thumbnail-image {
  width: 120px;
  height: 120px;
  border-radius: var(--radius-sm);
  object-fit: cover;
  border: 1px solid var(--color-border);
}

.thumbnail-remove {
  position: absolute;
  top: -6px;
  right: -6px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: none;
  background-color: var(--color-error, #d32f2f);
  color: white;
  font-size: 0.75rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
  padding: 0;
}

.thumbnail-remove:hover,
.thumbnail-remove:focus-visible {
  background-color: #b71c1c;
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.thumbnail-name {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin: 0;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  text-align: center;
}

.upload-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
}

.btn-sm {
  padding: var(--spacing-xs) var(--spacing-sm);
  font-size: 0.875rem;
}

.upload-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
  color: var(--color-text-secondary);
  padding: var(--spacing-lg);
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
