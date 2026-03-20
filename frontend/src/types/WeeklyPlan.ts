import type { WeeklyPlanItem } from './WeeklyPlanItem'

export interface WeeklyPlan {
  id: number
  items: WeeklyPlanItem[]
  createdAt: string
  updatedAt: string
}
