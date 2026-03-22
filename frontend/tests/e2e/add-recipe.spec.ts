import { test, expect } from '@playwright/test'

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'

test.describe('Add Recipe Flows', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(BASE_URL)
  })

  test('manual entry: fill form, submit, and verify in list', async ({ page }) => {
    // Navigate to recipe list
    await page.click('a[href="/recipes"]')
    await page.waitForURL('**/recipes')

    // Click the Add Recipe button
    await page.click('a[href="/recipes/new"]')
    await page.waitForURL('**/recipes/new')

    // Ensure Manual Entry tab is active
    await expect(page.getByRole('tab', { name: /manual entry/i })).toHaveAttribute('aria-selected', 'true')

    // Fill in the form
    await page.fill('#name', 'E2E Test Recipe')
    await page.fill('#description', 'A test recipe created by Playwright')
    await page.fill('#ingredients', 'Flour\nSugar\nButter\nEggs')
    await page.fill('#cookTime', '30')

    // Submit the form
    await page.click('button[type="submit"]')

    // Wait for navigation back to recipe list
    await page.waitForURL('**/recipes', { timeout: 5000 })

    // Verify the recipe appears in the list
    await expect(page.getByText('E2E Test Recipe')).toBeVisible()
  })

  test('manual entry: validation prevents empty name submission', async ({ page }) => {
    await page.goto(`${BASE_URL}/recipes/new`)

    // Try to submit with empty name
    await page.click('button[type="submit"]')

    // Verify validation error is shown
    await expect(page.getByText('Name is required')).toBeVisible()
  })

  test('image upload tab: shows upload UI', async ({ page }) => {
    await page.goto(`${BASE_URL}/recipes/new`)

    // Click the Upload Image tab
    await page.click('button:has-text("Upload Image")')

    // Verify image upload section is visible
    await expect(page.getByRole('tabpanel')).toBeVisible()
    await expect(page.locator('input[type="file"]')).toBeAttached()
  })

  test('delete recipe: confirmation dialog and removal', async ({ page }) => {
    await page.goto(`${BASE_URL}/recipes`)
    await page.waitForSelector('.recipe-card-wrapper')

    // Get the count of recipes before deletion
    const initialCount = await page.locator('.recipe-card-wrapper').count()

    // Click the first Delete button
    await page.locator('.recipe-card-wrapper').first().locator('button:has-text("Delete")').click()

    // Verify confirmation dialog appears
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByText('Delete Recipe')).toBeVisible()

    // Confirm deletion
    await page.click('[data-testid="confirm-btn"]')

    // Verify recipe is removed
    await expect(page.locator('.recipe-card-wrapper')).toHaveCount(initialCount - 1)
  })

  test('delete recipe: cancel preserves recipe', async ({ page }) => {
    await page.goto(`${BASE_URL}/recipes`)
    await page.waitForSelector('.recipe-card-wrapper')

    const initialCount = await page.locator('.recipe-card-wrapper').count()

    // Click the first Delete button
    await page.locator('.recipe-card-wrapper').first().locator('button:has-text("Delete")').click()

    // Verify dialog appears then cancel
    await expect(page.getByRole('dialog')).toBeVisible()
    await page.click('[data-testid="cancel-btn"]')

    // Verify recipe count unchanged
    await expect(page.locator('.recipe-card-wrapper')).toHaveCount(initialCount)
  })
})
