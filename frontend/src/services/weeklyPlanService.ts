import { apiClient } from './apiClient'
import { ApiClientError } from './apiClient'
import type { WeeklyPlan } from '@/types/WeeklyPlan'
import type { WeeklyPlanItem, AddToWeeklyPlanRequest } from '@/types/WeeklyPlanItem'

function rethrowWithAuthContext(error: unknown): never {
  if (error instanceof ApiClientError && error.statusCode === 401) {
    throw new ApiClientError('Authentication required', 401, error.data)
  }

  throw error
}

export const weeklyPlanService = {
  async getWeeklyPlan(): Promise<WeeklyPlan> {
    try {
      return await apiClient.get<WeeklyPlan>('/weekly-plan')
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async addToWeeklyPlan(request: AddToWeeklyPlanRequest): Promise<WeeklyPlanItem> {
    try {
      return await apiClient.post<WeeklyPlanItem, AddToWeeklyPlanRequest>('/weekly-plan/items', request)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async removeFromWeeklyPlan(itemId: number): Promise<void> {
    try {
      return await apiClient.delete<void>(`/weekly-plan/items/${itemId}`)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  }
}
