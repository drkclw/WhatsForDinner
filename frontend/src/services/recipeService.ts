import { apiClient } from './apiClient'
import type { Recipe, RecipeUpdateRequest, RecipeCreateRequest, RecipeImageExtractResult } from '@/types/Recipe'

export const recipeService = {
  async getRecipes(): Promise<Recipe[]> {
    return apiClient.get<Recipe[]>('/recipes')
  },

  async getRecipeById(id: number): Promise<Recipe> {
    return apiClient.get<Recipe>(`/recipes/${id}`)
  },

  async updateRecipe(id: number, request: RecipeUpdateRequest): Promise<Recipe> {
    return apiClient.put<Recipe, RecipeUpdateRequest>(`/recipes/${id}`, request)
  },

  async createRecipe(request: RecipeCreateRequest): Promise<Recipe> {
    return apiClient.post<Recipe, RecipeCreateRequest>('/recipes', request)
  },

  async deleteRecipe(id: number): Promise<void> {
    return apiClient.delete<void>(`/recipes/${id}`)
  },

  async extractFromImage(file: File): Promise<RecipeImageExtractResult> {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.postFormData<RecipeImageExtractResult>('/recipes/extract-from-image', formData)
  }
}
