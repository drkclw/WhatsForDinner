import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { apiClient, ApiClientError } from '@/services/apiClient'

export interface AuthUser {
  id: number
  email: string
  displayName: string
  avatarUrl?: string | null
}

interface GoogleSignInRequest {
  credential: string
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const sessionChecked = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => user.value !== null)

  async function handleGoogleCredential(response: google.accounts.id.CredentialResponse) {
    loading.value = true
    error.value = null

    try {
      user.value = await apiClient.post<AuthUser, GoogleSignInRequest>('/auth/google', {
        credential: response.credential
      })
    } catch (e) {
      if (e instanceof ApiClientError) {
        error.value = e.message
      } else {
        error.value = 'Sign in failed. Please try again.'
      }
      user.value = null
    } finally {
      loading.value = false
      sessionChecked.value = true
    }
  }

  async function restoreSession() {
    if (sessionChecked.value) {
      return
    }

    loading.value = true
    error.value = null

    try {
      user.value = await apiClient.get<AuthUser>('/auth/me')
    } catch {
      user.value = null
    } finally {
      loading.value = false
      sessionChecked.value = true
    }
  }

  async function logout() {
    try {
      await apiClient.post('/auth/logout', {})
    } finally {
      user.value = null
      error.value = null
      google.accounts.id.disableAutoSelect()
    }
  }

  function clearError() {
    error.value = null
  }

  return {
    user,
    sessionChecked,
    loading,
    error,
    isAuthenticated,
    handleGoogleCredential,
    restoreSession,
    logout,
    clearError
  }
})