import type { Page, Route } from '@playwright/test'

type RecipeRecord = {
  id: number
  name: string
  description: string | null
  ingredients: string | null
  preparation: string | null
  cookTimeMinutes: number | null
  createdAt: string
  updatedAt: string
}

const DEFAULT_USER = {
  id: 1,
  email: 'e2e-user@example.com',
  displayName: 'E2E User',
  avatarUrl: null,
}

function createSeedRecipes(): RecipeRecord[] {
  const now = new Date().toISOString()
  return [
    {
      id: 1,
      name: 'Seed Pasta',
      description: 'Baseline seeded recipe',
      ingredients: 'Pasta\nTomato Sauce',
      preparation: 'Boil pasta and add sauce',
      cookTimeMinutes: 20,
      createdAt: now,
      updatedAt: now,
    },
    {
      id: 2,
      name: 'Seed Salad',
      description: 'Fresh seeded salad',
      ingredients: 'Lettuce\nCucumber\nOlive Oil',
      preparation: 'Chop and toss',
      cookTimeMinutes: 10,
      createdAt: now,
      updatedAt: now,
    },
  ]
}

async function fulfillJson(route: Route, status: number, body: unknown) {
  await route.fulfill({
    status,
    contentType: 'application/json',
    body: JSON.stringify(body),
  })
}

export async function setupAuthenticatedSession(page: Page) {
  const recipes = createSeedRecipes()

  await page.route('**/api/auth/me', async route => {
    await fulfillJson(route, 200, DEFAULT_USER)
  })

  await page.route('**/api/weekly-plan', async route => {
    await fulfillJson(route, 200, {
      id: 1,
      items: [],
    })
  })

  await page.route('**/api/weekly-plan/items', async route => {
    if (route.request().method() === 'POST') {
      await fulfillJson(route, 201, {
        id: 1,
        recipeId: 1,
        recipeName: 'Seed Pasta',
        dayOfWeek: 1,
      })
      return
    }

    await route.fallback()
  })

  await page.route('**/api/weekly-plan/items/*', async route => {
    if (route.request().method() === 'DELETE') {
      await route.fulfill({ status: 204, body: '' })
      return
    }

    await route.fallback()
  })

  await page.route('**/api/recipes/extract-from-image', async route => {
    await fulfillJson(route, 200, {
      success: true,
      name: 'Extracted Recipe',
      description: 'Extracted description',
      ingredients: 'Extracted ingredients',
      preparation: 'Extracted preparation',
      cookTimeMinutes: 15,
      message: null,
    })
  })

  await page.route('**/api/recipes', async route => {
    const method = route.request().method()

    if (method === 'GET') {
      await fulfillJson(route, 200, recipes)
      return
    }

    if (method === 'POST') {
      const payload = (await route.request().postDataJSON()) as {
        name: string
        description?: string | null
        ingredients?: string | null
        preparation?: string | null
        cookTimeMinutes?: number | null
      }

      const now = new Date().toISOString()
      const createdRecipe: RecipeRecord = {
        id: recipes.length ? Math.max(...recipes.map(recipe => recipe.id)) + 1 : 1,
        name: payload.name,
        description: payload.description ?? null,
        ingredients: payload.ingredients ?? null,
        preparation: payload.preparation ?? null,
        cookTimeMinutes: payload.cookTimeMinutes ?? null,
        createdAt: now,
        updatedAt: now,
      }

      recipes.unshift(createdRecipe)
      await fulfillJson(route, 201, createdRecipe)
      return
    }

    await route.fallback()
  })

  await page.route('**/api/recipes/*', async route => {
    const request = route.request()
    const method = request.method()
    const url = new URL(request.url())
    const idText = url.pathname.split('/').pop()
    const id = Number(idText)

    if (!Number.isInteger(id)) {
      await route.fallback()
      return
    }

    const recipeIndex = recipes.findIndex(recipe => recipe.id === id)

    if (method === 'GET') {
      if (recipeIndex === -1) {
        await route.fulfill({ status: 404, body: '' })
        return
      }

      await fulfillJson(route, 200, recipes[recipeIndex])
      return
    }

    if (method === 'DELETE') {
      if (recipeIndex === -1) {
        await route.fulfill({ status: 404, body: '' })
        return
      }

      recipes.splice(recipeIndex, 1)
      await route.fulfill({ status: 204, body: '' })
      return
    }

    await route.fallback()
  })
}