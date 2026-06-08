<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView } from 'vue-router'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const router = useRouter()

const initials = computed(() => {
  if (!authStore.user?.displayName) {
    return 'U'
  }

  return authStore.user.displayName
    .split(' ')
    .map(n => n[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()
})

async function onSignOut() {
  await authStore.logout()
  await router.replace('/login')
}
</script>

<template>
  <div class="app">
    <a href="#main-content" class="skip-link">Skip to main content</a>
    <header class="app-header">
      <nav class="nav" aria-label="Main navigation">
        <RouterLink to="/" class="nav-brand">WhatsForDinner</RouterLink>
        <ul v-if="authStore.isAuthenticated" class="nav-links" role="menubar">
          <li role="none">
            <RouterLink to="/" role="menuitem">Weekly Plan</RouterLink>
          </li>
          <li role="none">
            <RouterLink to="/recipes" role="menuitem">Recipes</RouterLink>
          </li>
        </ul>
        <div v-if="authStore.isAuthenticated" class="user-menu">
          <button class="avatar-button" type="button" :aria-label="`Signed in as ${authStore.user?.displayName}`">
            {{ initials }}
          </button>
          <div class="dropdown" role="menu">
            <span class="user-name">{{ authStore.user?.displayName }}</span>
            <button type="button" class="signout" @click="onSignOut">Sign out</button>
          </div>
        </div>
      </nav>
    </header>
    <main id="main-content" class="main-content" tabindex="-1">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-header {
  background-color: var(--color-primary);
  color: white;
  padding: var(--spacing-md);
}

.nav {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.nav-brand {
  font-size: 1.5rem;
  font-weight: bold;
  color: white;
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: var(--spacing-md);
  list-style: none;
  margin: 0;
  padding: 0;
  margin-left: auto;
}

.nav-links a {
  color: white;
  text-decoration: none;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-sm);
  transition: background-color 0.2s;
}

.nav-links a:hover,
.nav-links a.router-link-exact-active {
  background-color: rgba(255, 255, 255, 0.2);
  text-decoration: none;
}

.main-content {
  flex: 1;
  padding: var(--spacing-lg);
}

.user-menu {
  position: relative;
}

.avatar-button {
  border: none;
  border-radius: 999px;
  width: 2.25rem;
  height: 2.25rem;
  font-weight: 700;
  cursor: pointer;
}

.dropdown {
  position: absolute;
  right: 0;
  top: calc(100% + 0.5rem);
  background: white;
  color: var(--color-text);
  box-shadow: var(--shadow-md);
  border-radius: var(--radius-sm);
  padding: var(--spacing-sm);
  display: none;
  min-width: 180px;
}

.user-menu:hover .dropdown,
.user-menu:focus-within .dropdown {
  display: grid;
  gap: var(--spacing-xs);
}

.user-name {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.signout {
  border: none;
  background: var(--color-error);
  color: white;
  border-radius: var(--radius-sm);
  padding: var(--spacing-xs) var(--spacing-sm);
  cursor: pointer;
}

@media (max-width: 768px) {
  .nav {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .main-content {
    padding: var(--spacing-md);
  }
}
</style>
