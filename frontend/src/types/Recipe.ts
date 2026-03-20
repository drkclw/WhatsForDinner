export interface Recipe {
  id: number
  name: string
  description: string | null
  ingredients: string | null
  cookTimeMinutes: number | null
  createdAt: string
  updatedAt: string
}

export interface RecipeUpdateRequest {
  name: string
  description?: string | null
  ingredients?: string | null
  cookTimeMinutes?: number | null
}
