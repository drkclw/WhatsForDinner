import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Recipe } from '@/types/Recipe'
import type { RecipeUpdateRequest, RecipeCreateRequest } from '@/types/Recipe'
import { recipeService } from '@/services/recipeService'
import { ApiClientError } from '@/services/apiClient'
import { useWeeklyPlanStore } from '@/stores/weeklyPlanStore'

export const useRecipeStore = defineStore('recipes', () => {
  const recipes = ref<Recipe[]>([])
  const currentRecipe = ref<Recipe | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const recipeCount = computed(() => recipes.value.length)

  async function fetchRecipes() {
    isLoading.value = true
    error.value = null
    
    try {
      recipes.value = await recipeService.getRecipes()
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to load recipes'
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchRecipeById(id: number) {
    isLoading.value = true
    error.value = null
    
    try {
      currentRecipe.value = await recipeService.getRecipeById(id)
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to load recipe'
      }
    } finally {
      isLoading.value = false
    }
  }

  async function updateRecipe(id: number, request: RecipeUpdateRequest): Promise<Recipe | null> {
    error.value = null
    
    try {
      const updated = await recipeService.updateRecipe(id, request)
      
      // Update in local list
      const index = recipes.value.findIndex(r => r.id === id)
      if (index !== -1) {
        recipes.value[index] = updated
      }
      
      // Update current recipe if it's the same
      if (currentRecipe.value?.id === id) {
        currentRecipe.value = updated
      }
      
      return updated
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to update recipe'
      }
      return null
    }
  }

  async function createRecipe(request: RecipeCreateRequest): Promise<Recipe | null> {
    error.value = null
    
    try {
      const created = await recipeService.createRecipe(request)
      recipes.value.unshift(created)
      return created
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to create recipe'
      }
      return null
    }
  }

  async function deleteRecipe(id: number): Promise<boolean> {
    error.value = null
    
    try {
      await recipeService.deleteRecipe(id)
      recipes.value = recipes.value.filter(r => r.id !== id)
      
      if (currentRecipe.value?.id === id) {
        currentRecipe.value = null
      }

      // Refresh weekly plan to reflect cascade-removed items
      try {
        const weeklyPlanStore = useWeeklyPlanStore()
        await weeklyPlanStore.fetchWeeklyPlan()
      } catch {
        // Non-critical: weekly plan refresh failure shouldn't block delete success
      }
      
      return true
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to delete recipe'
      }
      return false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentRecipe() {
    currentRecipe.value = null
  }

  return {
    recipes,
    currentRecipe,
    recipeCount,
    isLoading,
    error,
    fetchRecipes,
    fetchRecipeById,
    updateRecipe,
    createRecipe,
    deleteRecipe,
    clearError,
    clearCurrentRecipe
  }
})
