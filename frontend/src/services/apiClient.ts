import type { ApiError } from '@/types/ApiError'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

export class ApiClientError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public data?: ApiError
  ) {
    super(message)
    this.name = 'ApiClientError'
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorData: ApiError | undefined
    try {
      errorData = await response.json()
    } catch {
      // Response is not JSON
    }
    throw new ApiClientError(
      errorData?.message || `HTTP error ${response.status}`,
      response.status,
      errorData
    )
  }
  
  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T
  }
  
  return response.json()
}

export const apiClient = {
  async get<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    })
    return handleResponse<T>(response)
  },

  async post<T, D = unknown>(endpoint: string, data: D): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
    return handleResponse<T>(response)
  },

  async put<T, D = unknown>(endpoint: string, data: D): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
    return handleResponse<T>(response)
  },

  async delete<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      }
    })
    return handleResponse<T>(response)
  }
}
