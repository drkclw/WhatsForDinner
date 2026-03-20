import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { WeeklyPlan } from '@/types/WeeklyPlan'
import type { WeeklyPlanItem } from '@/types/WeeklyPlanItem'
import { weeklyPlanService } from '@/services/weeklyPlanService'
import { ApiClientError } from '@/services/apiClient'

export const useWeeklyPlanStore = defineStore('weeklyPlan', () => {
  const weeklyPlan = ref<WeeklyPlan | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const items = computed(() => weeklyPlan.value?.items ?? [])
  const isEmpty = computed(() => items.value.length === 0)

  async function fetchWeeklyPlan() {
    isLoading.value = true
    error.value = null
    
    try {
      weeklyPlan.value = await weeklyPlanService.getWeeklyPlan()
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to load weekly plan'
      }
    } finally {
      isLoading.value = false
    }
  }

  async function addRecipe(recipeId: number): Promise<WeeklyPlanItem | null> {
    error.value = null
    
    try {
      const item = await weeklyPlanService.addToWeeklyPlan({ recipeId })
      if (weeklyPlan.value) {
        weeklyPlan.value.items.push(item)
      }
      return item
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to add recipe to plan'
      }
      return null
    }
  }

  async function removeRecipe(itemId: number): Promise<boolean> {
    error.value = null
    
    try {
      await weeklyPlanService.removeFromWeeklyPlan(itemId)
      if (weeklyPlan.value) {
        weeklyPlan.value.items = weeklyPlan.value.items.filter(i => i.id !== itemId)
      }
      return true
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Failed to remove recipe from plan'
      }
      return false
    }
  }

  function clearError() {
    error.value = null
  }

  return {
    weeklyPlan,
    items,
    isEmpty,
    isLoading,
    error,
    fetchWeeklyPlan,
    addRecipe,
    removeRecipe,
    clearError
  }
})
