import { apiClient } from './apiClient'
import { ApiClientError } from './apiClient'
import type { Recipe, RecipeUpdateRequest, RecipeCreateRequest, RecipeImageExtractResult } from '@/types/Recipe'

function rethrowWithAuthContext(error: unknown): never {
  if (error instanceof ApiClientError && error.statusCode === 401) {
    throw new ApiClientError('Authentication required', 401, error.data)
  }

  throw error
}

export const recipeService = {
  async getRecipes(): Promise<Recipe[]> {
    try {
      return await apiClient.get<Recipe[]>('/recipes')
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async getRecipeById(id: number): Promise<Recipe> {
    try {
      return await apiClient.get<Recipe>(`/recipes/${id}`)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async updateRecipe(id: number, request: RecipeUpdateRequest): Promise<Recipe> {
    try {
      return await apiClient.put<Recipe, RecipeUpdateRequest>(`/recipes/${id}`, request)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async createRecipe(request: RecipeCreateRequest): Promise<Recipe> {
    try {
      return await apiClient.post<Recipe, RecipeCreateRequest>('/recipes', request)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async deleteRecipe(id: number): Promise<void> {
    try {
      return await apiClient.delete<void>(`/recipes/${id}`)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  },

  async extractFromImage(files: File[]): Promise<RecipeImageExtractResult> {
    const formData = new FormData()
    for (const file of files) {
      formData.append('files', file)
    }
    try {
      return await apiClient.postFormData<RecipeImageExtractResult>('/recipes/extract-from-image', formData)
    } catch (error) {
      rethrowWithAuthContext(error)
    }
  }
}
