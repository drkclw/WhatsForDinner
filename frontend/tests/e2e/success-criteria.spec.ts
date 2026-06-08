import { expect, test, type Page } from '@playwright/test'

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'

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

function seedAuthAndDataRoutes(page: Page, recipes: RecipeRecord[] = []) {
  let signedIn = false
  const user = {
    id: 1,
    email: 'metrics-user@example.com',
    displayName: 'Metrics User',
    avatarUrl: null,
  }

  const addRoutes = async () => {
    await page.route('**/api/auth/me', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(user) })
    })

    await page.route('**/api/auth/google', async route => {
      signedIn = true
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(user) })
    })

    await page.route('**/api/auth/logout', async route => {
      signedIn = false
      await route.fulfill({ status: 204, body: '' })
    })

    await page.route('**/api/weekly-plan', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ id: 1, items: [] }) })
    })

    await page.route('**/api/weekly-plan/items', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({ id: 1, recipeId: 1, recipeName: 'Any', dayOfWeek: 1 }),
        })
        return
      }

      await route.fallback()
    })

    await page.route('**/api/weekly-plan/items/*', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      if (route.request().method() === 'DELETE') {
        await route.fulfill({ status: 204, body: '' })
        return
      }

      await route.fallback()
    })

    await page.route('**/api/recipes', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      const method = route.request().method()

      if (method === 'GET') {
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(recipes) })
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
        await route.fulfill({ status: 201, contentType: 'application/json', body: JSON.stringify(createdRecipe) })
        return
      }

      await route.fallback()
    })

    await page.route('**/api/recipes/*', async route => {
      if (!signedIn) {
        await route.fulfill({ status: 401, contentType: 'application/json', body: JSON.stringify({ message: 'Unauthorized' }) })
        return
      }

      await route.fallback()
    })
  }

  const signInViaGoogleButton = async () => {
    await page.getByRole('button', { name: 'Sign in with Google' }).click()
  }

  return { addRoutes, signInViaGoogleButton }
}

async function mockGoogleSignInButton(page: Page) {
  await page.addInitScript(() => {
    const globalObject = window as typeof window & {
      __gisCallback?: (response: { credential: string }) => Promise<void> | void
    }

    globalObject.google = {
      accounts: {
        id: {
          initialize: ({ callback }: { callback: (response: { credential: string }) => Promise<void> | void }) => {
            globalObject.__gisCallback = callback
          },
          renderButton: (container: HTMLElement) => {
            const button = document.createElement('button')
            button.type = 'button'
            button.textContent = 'Sign in with Google'
            button.setAttribute('aria-label', 'Sign in with Google')
            button.addEventListener('click', async () => {
              await globalObject.__gisCallback?.({ credential: 'dummy-google-id-token' })
            })
            container.replaceChildren(button)
          },
          disableAutoSelect: () => {
            // no-op for tests
          },
        },
      },
    } as unknown as typeof google
  })
}

test.describe('Success Criteria Validation', () => {
  test('SC-001: sign-in flow completes under 60 seconds', async ({ page }) => {
    await mockGoogleSignInButton(page)
    const { addRoutes, signInViaGoogleButton } = seedAuthAndDataRoutes(page)
    await addRoutes()

    await page.goto(`${BASE_URL}/login`)

    const startedAt = Date.now()
    await signInViaGoogleButton()
    await page.waitForURL(`${BASE_URL}/`)
    const elapsedMs = Date.now() - startedAt

    console.log(`SC001_SIGNIN_MS=${elapsedMs}`)
    expect(elapsedMs).toBeLessThan(60000)
  })

  test('SC-004: returning user data is present after re-authentication', async ({ page }) => {
    await mockGoogleSignInButton(page)
    const recipes: RecipeRecord[] = []
    const { addRoutes, signInViaGoogleButton } = seedAuthAndDataRoutes(page, recipes)
    await addRoutes()

    const recipeName = 'Returning User Persistence Recipe'

    await page.goto(`${BASE_URL}/login`)
    await signInViaGoogleButton()
    await page.waitForURL(`${BASE_URL}/`)

    await page.goto(`${BASE_URL}/recipes/new`)
    await page.fill('#name', recipeName)
    await page.fill('#description', 'Persisted across re-auth')
    await page.fill('#ingredients', 'Ingredient A')
    await page.click('button[type="submit"]')
    await page.waitForURL('**/recipes')
    await expect(page.getByText(recipeName)).toBeVisible()

    const avatarButton = page.locator('.avatar-button')
    await avatarButton.hover()
    await page.getByRole('button', { name: 'Sign out' }).click()
    await page.waitForURL(`${BASE_URL}/login`)

    await page.evaluate(async () => {
      await fetch('/api/auth/google', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ credential: 'dummy-google-id-token' }),
      })
    })

    await page.goto(`${BASE_URL}/recipes`)
    await expect(page).toHaveURL(`${BASE_URL}/recipes`)
    await expect(page.getByText(recipeName)).toBeVisible()

    console.log('SC004_PERSISTENCE=PASS')
  })

  test('SC-005: login page FCP is <= 2s on simulated 3G', async ({ page }) => {
    await mockGoogleSignInButton(page)
    const { addRoutes } = seedAuthAndDataRoutes(page)
    await addRoutes()

    const client = await page.context().newCDPSession(page)
    await client.send('Network.enable')
    await client.send('Network.emulateNetworkConditions', {
      offline: false,
      latency: 150,
      downloadThroughput: 750 * 1024 / 8,
      uploadThroughput: 250 * 1024 / 8,
      connectionType: 'cellular3g',
    })

    await page.goto(`${BASE_URL}/login`, { waitUntil: 'domcontentloaded' })

    const fcpMs = await page.evaluate(async () => {
      const existing = performance.getEntriesByName('first-contentful-paint')[0]
      if (existing) {
        return existing.startTime
      }

      return await new Promise<number>(resolve => {
        const observer = new PerformanceObserver(entryList => {
          const fcpEntry = entryList.getEntries().find(entry => entry.name === 'first-contentful-paint')
          if (fcpEntry) {
            observer.disconnect()
            resolve(fcpEntry.startTime)
          }
        })

        observer.observe({ type: 'paint', buffered: true })
        setTimeout(() => {
          observer.disconnect()
          const fallback = performance.getEntriesByName('first-contentful-paint')[0]
          resolve(fallback ? fallback.startTime : -1)
        }, 5000)
      })
    })

    await client.send('Network.disable')

    console.log(`SC005_FCP_MS=${Math.round(fcpMs)}`)
    expect(fcpMs).toBeGreaterThan(0)
    expect(fcpMs).toBeLessThanOrEqual(2000)
  })
})