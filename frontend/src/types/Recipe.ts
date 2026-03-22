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

export interface RecipeCreateRequest {
  name: string
  description?: string | null
  ingredients?: string | null
  cookTimeMinutes?: number | null
}

export interface RecipeImageExtractResult {
  success: boolean
  name?: string | null
  description?: string | null
  ingredients?: string | null
  cookTimeMinutes?: number | null
  message?: string | null
}
