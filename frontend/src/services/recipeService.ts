import { apiClient } from './apiClient'
import type { Recipe, RecipeUpdateRequest } from '@/types/Recipe'

export const recipeService = {
  async getRecipes(): Promise<Recipe[]> {
    return apiClient.get<Recipe[]>('/recipes')
  },

  async getRecipeById(id: number): Promise<Recipe> {
    return apiClient.get<Recipe>(`/recipes/${id}`)
  },

  async updateRecipe(id: number, request: RecipeUpdateRequest): Promise<Recipe> {
    return apiClient.put<Recipe, RecipeUpdateRequest>(`/recipes/${id}`, request)
  }
}
