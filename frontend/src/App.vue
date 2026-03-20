<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app">
    <a href="#main-content" class="skip-link">Skip to main content</a>
    <header class="app-header">
      <nav class="nav" aria-label="Main navigation">
        <RouterLink to="/" class="nav-brand">WhatsForDinner</RouterLink>
        <ul class="nav-links" role="menubar">
          <li role="none">
            <RouterLink to="/" role="menuitem">Weekly Plan</RouterLink>
          </li>
          <li role="none">
            <RouterLink to="/recipes" role="menuitem">Recipes</RouterLink>
          </li>
        </ul>
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
  justify-content: space-between;
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
