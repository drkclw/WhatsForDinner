import { expect, test } from '@playwright/test'

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'

async function mockAuthenticatedSession(page: import('@playwright/test').Page) {
  let signedIn = true

  await page.route('**/api/auth/me', async route => {
    if (!signedIn) {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Unauthorized' })
      })
      return
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 1,
        email: 'user1@example.com',
        displayName: 'User One',
        avatarUrl: null
      })
    })
  })

  await page.route('**/api/auth/logout', async route => {
    signedIn = false
    await route.fulfill({
      status: 204,
      body: ''
    })
  })

  await page.route('**/api/recipes', async route => {
    if (!signedIn) {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Unauthorized' })
      })
      return
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([])
    })
  })

  await page.route('**/api/weekly-plan', async route => {
    if (!signedIn) {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Unauthorized' })
      })
      return
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 1,
        items: []
      })
    })
  })
}

test.describe('Authentication', () => {
  test('unauthenticated users are redirected to login', async ({ page }) => {
    await page.goto(`${BASE_URL}/recipes`)

    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('heading', { name: 'WhatsForDinner' })).toBeVisible()
    await expect(page.getByLabel('Sign in with Google')).toBeAttached()
  })

  test('login page renders funny description', async ({ page }) => {
    await page.goto(`${BASE_URL}/login`)

    await expect(page.getByText('before your stomach files a bug report')).toBeVisible()
  })

  test('sign out redirects to login and blocks protected routes', async ({ page }) => {
    await mockAuthenticatedSession(page)

    await page.goto(`${BASE_URL}/`)
    await expect(page).toHaveURL(`${BASE_URL}/`)

    const avatarButton = page.locator('.avatar-button')
    await expect(avatarButton).toBeVisible()
    await avatarButton.hover()

    const logoutRequest = page.waitForRequest('**/api/auth/logout')
    await page.getByRole('button', { name: 'Sign out' }).click()
    await logoutRequest

    await page.goto(`${BASE_URL}/recipes`)
    await expect(page).toHaveURL(`${BASE_URL}/login`)
  })
})