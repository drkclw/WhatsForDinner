import { test, expect } from '@playwright/test'
import { setupAuthenticatedSession } from './helpers/authSession'

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'

function createImageFixture(name: string) {
  // Minimal JPEG header bytes are sufficient for upload validation in UI tests.
  const jpegBytes = Buffer.from('/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAP//////////////////////////////////////////////////////////////////////////////////////2wBDAf//////////////////////////////////////////////////////////////////////////////////////wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAf/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAABQb/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCeAAX/2Q==', 'base64')
  return {
    name,
    mimeType: 'image/jpeg',
    buffer: jpegBytes,
  }
}

test.describe('Multi-Image Recipe Extraction', () => {
  test.beforeEach(async ({ page }) => {
    await setupAuthenticatedSession(page)
    await page.goto(`${BASE_URL}/recipes/new`)
    // Switch to Upload Image tab
    await page.click('button:has-text("Upload Image")')
  })

  test('upload multiple images and see thumbnails', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]')

    // Upload 2 images via the file input
    await fileInput.setInputFiles([
      createImageFixture('test-image-1.jpg'),
      createImageFixture('test-image-2.jpg'),
    ])

    // Verify thumbnails are shown
    const thumbnails = page.locator('.thumbnail-item')
    await expect(thumbnails).toHaveCount(2)

    // Verify remove buttons exist with correct aria-labels
    await expect(page.locator('button[aria-label="Remove image 1"]')).toBeVisible()
    await expect(page.locator('button[aria-label="Remove image 2"]')).toBeVisible()

    // Verify Extract button is visible
    await expect(page.getByRole('button', { name: 'Extract Recipe' })).toBeVisible()
  })

  test('remove an image from the upload set', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]')

    await fileInput.setInputFiles([
      createImageFixture('test-image-1.jpg'),
      createImageFixture('test-image-2.jpg'),
    ])

    await expect(page.locator('.thumbnail-item')).toHaveCount(2)

    // Remove the first image
    await page.click('button[aria-label="Remove image 1"]')

    // Only 1 thumbnail should remain
    await expect(page.locator('.thumbnail-item')).toHaveCount(1)
  })

  test('removing all images shows upload prompt again', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]')

    await fileInput.setInputFiles([
      createImageFixture('test-image-1.jpg'),
    ])

    await expect(page.locator('.thumbnail-item')).toHaveCount(1)

    // Remove the only image
    await page.click('button[aria-label="Remove image 1"]')

    // Upload area should reappear
    await expect(page.locator('.upload-area')).toBeVisible()
    await expect(page.locator('.thumbnail-grid')).not.toBeVisible()
  })

  test('preparation field exists in the recipe form', async ({ page }) => {
    // Switch back to Manual Entry tab
    await page.click('button:has-text("Manual Entry")')

    // Verify preparation textarea exists
    const preparationField = page.locator('#preparation')
    await expect(preparationField).toBeVisible()

    // Fill preparation and verify
    await preparationField.fill('Step 1: Preheat oven to 350°F\nStep 2: Mix ingredients')
    await expect(preparationField).toHaveValue('Step 1: Preheat oven to 350°F\nStep 2: Mix ingredients')
  })

  test('manual entry with preparation: fill all fields and save', async ({ page }) => {
    // Switch to Manual Entry tab
    await page.click('button:has-text("Manual Entry")')

    // Fill all fields including preparation
    await page.fill('#name', 'E2E Preparation Test Recipe')
    await page.fill('#description', 'A test recipe with preparation steps')
    await page.fill('#ingredients', 'Flour\nSugar\nButter')
    await page.fill('#preparation', 'Step 1: Mix dry ingredients\nStep 2: Add butter\nStep 3: Bake at 350°F for 25 minutes')
    await page.fill('#cookTime', '25')

    // Submit the form
    await page.click('button[type="submit"]')

    // Wait for navigation back to recipe list
    await page.waitForURL('**/recipes', { timeout: 5000 })

    // Verify the recipe appears in the list
    await expect(page.getByText('E2E Preparation Test Recipe')).toBeVisible()
  })
})
