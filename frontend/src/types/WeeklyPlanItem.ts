import type { Recipe } from './Recipe'

export interface WeeklyPlanItem {
  id: number
  recipe: Recipe
  addedAt: string
}

export interface AddToWeeklyPlanRequest {
  recipeId: number
}
