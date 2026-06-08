<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const router = useRouter()
const buttonContainer = ref<HTMLElement | null>(null)
const hasGoogleClientId = Boolean(import.meta.env.VITE_GOOGLE_CLIENT_ID)

onMounted(() => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID

  if (!clientId) {
    authStore.clearError()
    return
  }

  google.accounts.id.initialize({
    client_id: clientId,
    callback: async (response) => {
      await authStore.handleGoogleCredential(response)
      if (authStore.isAuthenticated) {
        await router.replace('/')
      }
    },
    auto_select: false,
    cancel_on_tap_outside: true
  })

  if (buttonContainer.value) {
    google.accounts.id.renderButton(buttonContainer.value, {
      theme: 'outline',
      size: 'large',
      text: 'signin_with',
      width: 280
    })
  }
})
</script>

<template>
  <section class="login-wrap" aria-labelledby="login-heading">
    <div class="login-card">
      <h1 id="login-heading">WhatsForDinner</h1>
      <p class="tagline">
        We solve the nightly mystery of "what do we eat?" before your stomach files a bug report.
      </p>

      <p v-if="authStore.error" class="error" role="alert">{{ authStore.error }}</p>
      <p v-else-if="!hasGoogleClientId" class="error" role="alert">
        Google Sign-In is not configured. Set VITE_GOOGLE_CLIENT_ID.
      </p>

      <div ref="buttonContainer" class="google-button" aria-label="Sign in with Google" />
    </div>
  </section>
</template>

<style scoped>
.login-wrap {
  min-height: calc(100vh - 120px);
  display: grid;
  place-items: center;
}

.login-card {
  width: min(560px, 100%);
  background: white;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
  padding: var(--spacing-xl);
  text-align: center;
}

h1 {
  margin: 0 0 var(--spacing-sm) 0;
}

.tagline {
  margin: 0 0 var(--spacing-lg) 0;
  color: var(--color-text-secondary);
}

.error {
  color: var(--color-error);
  margin-bottom: var(--spacing-md);
}

.google-button {
  display: flex;
  justify-content: center;
}
</style>