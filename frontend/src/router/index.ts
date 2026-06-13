import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/',
      name: 'weekly-plan',
      component: () => import('@/views/WeeklyPlanView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/recipes',
      name: 'recipes',
      component: () => import('@/views/RecipeListView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/recipes/new',
      name: 'recipe-create',
      component: () => import('@/views/RecipeCreateView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/recipes/:id/edit',
      name: 'recipe-edit',
      component: () => import('@/views/RecipeEditView.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

router.beforeEach(async to => {
  const authStore = useAuthStore()

  if (!authStore.sessionChecked) {
    await authStore.restoreSession()
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    return { name: 'weekly-plan' }
  }

  return true
})

export default router
