export interface ApiError {
  message: string
}

export interface ValidationError extends ApiError {
  errors: Record<string, string[]>
}

export function isValidationError(error: ApiError): error is ValidationError {
  return 'errors' in error && typeof error.errors === 'object'
}
