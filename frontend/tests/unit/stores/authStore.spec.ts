import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useAuthStore } from '@/stores/authStore'

const mockedApiClient = vi.hoisted(() => ({
  get: vi.fn(),
  post: vi.fn()
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: mockedApiClient,
  ApiClientError: class extends Error {}
}))

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mockedApiClient.get.mockReset()
    mockedApiClient.post.mockReset()

    ;(globalThis as any).google = {
      accounts: {
        id: {
          disableAutoSelect: vi.fn()
        }
      }
    }
  })

  it('restores session when /auth/me succeeds', async () => {
    mockedApiClient.get.mockResolvedValue({
      id: 1,
      email: 'user@example.com',
      displayName: 'User One',
      avatarUrl: 'https://example.com/a.png'
    })

    const store = useAuthStore()
    await store.restoreSession()

    expect(store.isAuthenticated).toBe(true)
    expect(store.user?.displayName).toBe('User One')
    expect(store.sessionChecked).toBe(true)
  })

  it('handles google sign-in success', async () => {
    mockedApiClient.post.mockResolvedValue({
      id: 1,
      email: 'user@example.com',
      displayName: 'User One',
      avatarUrl: null
    })

    const store = useAuthStore()
    await store.handleGoogleCredential({ credential: 'test-credential' })

    expect(store.isAuthenticated).toBe(true)
    expect(store.error).toBeNull()
  })

  it('handles google sign-in failure', async () => {
    mockedApiClient.post.mockRejectedValue(new Error('invalid credential'))

    const store = useAuthStore()
    await store.handleGoogleCredential({ credential: 'bad' })

    expect(store.isAuthenticated).toBe(false)
    expect(store.error).toBeTruthy()
  })

  it('logs out and clears local state', async () => {
    mockedApiClient.post.mockResolvedValue(undefined)

    const store = useAuthStore()
    store.user = {
      id: 1,
      email: 'user@example.com',
      displayName: 'User One',
      avatarUrl: null
    }

    await store.logout()

    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
  })
})