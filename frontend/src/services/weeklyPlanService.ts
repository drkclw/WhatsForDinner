import { apiClient } from './apiClient'
import type { WeeklyPlan } from '@/types/WeeklyPlan'
import type { WeeklyPlanItem, AddToWeeklyPlanRequest } from '@/types/WeeklyPlanItem'

export const weeklyPlanService = {
  async getWeeklyPlan(): Promise<WeeklyPlan> {
    return apiClient.get<WeeklyPlan>('/weekly-plan')
  },

  async addToWeeklyPlan(request: AddToWeeklyPlanRequest): Promise<WeeklyPlanItem> {
    return apiClient.post<WeeklyPlanItem, AddToWeeklyPlanRequest>('/weekly-plan/items', request)
  },

  async removeFromWeeklyPlan(itemId: number): Promise<void> {
    return apiClient.delete<void>(`/weekly-plan/items/${itemId}`)
  }
}
