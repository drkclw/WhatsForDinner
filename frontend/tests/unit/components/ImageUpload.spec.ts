import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import ImageUpload from '@/components/ImageUpload.vue'

// Mock URL.createObjectURL and revokeObjectURL for jsdom
beforeEach(() => {
  global.URL.createObjectURL = vi.fn(() => 'blob:mock-url')
  global.URL.revokeObjectURL = vi.fn()
})

function createFile(name: string, type = 'image/jpeg', sizeMB = 1): File {
  const content = new Uint8Array(sizeMB * 1024 * 1024)
  return new File([content], name, { type })
}

async function addFilesViaInput(wrapper: ReturnType<typeof mount>, files: File[]) {
  const input = wrapper.find('input[type="file"]')
  Object.defineProperty(input.element, 'files', { value: files, configurable: true })
  await input.trigger('change')
}

describe('ImageUpload', () => {
  it('renders the file input with multiple attribute', () => {
    const wrapper = mount(ImageUpload)
    const input = wrapper.find('input[type="file"]')
    expect(input.exists()).toBe(true)
    expect(input.attributes('multiple')).toBeDefined()
  })

  it('accepts only JPEG, PNG, and WebP files', () => {
    const wrapper = mount(ImageUpload)
    const input = wrapper.find('input[type="file"]')
    expect(input.attributes('accept')).toBe('image/jpeg,image/png,image/webp')
  })

  it('shows error for unsupported file type', async () => {
    const wrapper = mount(ImageUpload)

    const file = new File(['content'], 'document.pdf', { type: 'application/pdf' })
    await addFilesViaInput(wrapper, [file])

    expect(wrapper.text()).toContain('not a supported format')
    expect(wrapper.emitted('files-changed')).toBeFalsy()
  })

  it('shows error for file exceeding 10 MB', async () => {
    const wrapper = mount(ImageUpload)

    const file = createFile('large-image.jpg', 'image/jpeg', 11)
    await addFilesViaInput(wrapper, [file])

    expect(wrapper.text()).toContain('too large')
    expect(wrapper.emitted('files-changed')).toBeFalsy()
  })

  it('emits files-changed with file array for valid file', async () => {
    const wrapper = mount(ImageUpload)

    const file = createFile('recipe.jpg')
    await addFilesViaInput(wrapper, [file])

    const emitted = wrapper.emitted('files-changed')
    expect(emitted).toBeTruthy()
    expect(emitted![0][0]).toEqual([file])
  })

  it('shows thumbnails after adding files', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg'), createFile('b.jpg')])

    const thumbnails = wrapper.findAll('.thumbnail-item')
    expect(thumbnails).toHaveLength(2)
  })

  it('rejects more than 5 files at once', async () => {
    const wrapper = mount(ImageUpload)

    const files = Array.from({ length: 6 }, (_, i) => createFile(`img${i}.jpg`))
    await addFilesViaInput(wrapper, files)

    expect(wrapper.text()).toContain('max 5')
    expect(wrapper.emitted('files-changed')).toBeFalsy()
  })

  it('prevents exceeding 5 files with cumulative adds', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg'), createFile('b.jpg'), createFile('c.jpg')])
    await addFilesViaInput(wrapper, [createFile('d.jpg'), createFile('e.jpg'), createFile('f.jpg')])

    expect(wrapper.text()).toContain('more image')
    // Only 3 from the first batch
    const emitted = wrapper.emitted('files-changed')!
    expect(emitted[emitted.length - 1][0]).toHaveLength(3)
  })

  it('removes an image and emits updated file list', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg'), createFile('b.jpg'), createFile('c.jpg')])

    // Remove the second image
    const removeButtons = wrapper.findAll('.thumbnail-remove')
    expect(removeButtons).toHaveLength(3)
    await removeButtons[1].trigger('click')

    const thumbnails = wrapper.findAll('.thumbnail-item')
    expect(thumbnails).toHaveLength(2)

    const emitted = wrapper.emitted('files-changed')!
    const lastEmit = emitted[emitted.length - 1][0] as File[]
    expect(lastEmit).toHaveLength(2)
    expect(lastEmit[0].name).toBe('a.jpg')
    expect(lastEmit[1].name).toBe('c.jpg')

    expect(global.URL.revokeObjectURL).toHaveBeenCalled()
  })

  it('shows upload prompt when all images are removed', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg')])
    expect(wrapper.find('.thumbnail-grid').exists()).toBe(true)

    await wrapper.find('.thumbnail-remove').trigger('click')
    expect(wrapper.find('.thumbnail-grid').exists()).toBe(false)
    expect(wrapper.find('.upload-area').exists()).toBe(true)
  })

  it('remove buttons have accessible aria-labels', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg'), createFile('b.jpg')])

    const removeButtons = wrapper.findAll('.thumbnail-remove')
    expect(removeButtons[0].attributes('aria-label')).toBe('Remove image 1')
    expect(removeButtons[1].attributes('aria-label')).toBe('Remove image 2')
  })

  it('emits extract event when Extract button is clicked', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg')])

    const buttons = wrapper.findAll('button')
    const extractBtn = buttons[buttons.length - 1]

    expect(extractBtn.exists()).toBe(true)
    await extractBtn.trigger('click')

    expect(wrapper.emitted('extract')).toBeTruthy()
  })

  it('shows loading state with custom message', () => {
    const wrapper = mount(ImageUpload, {
      props: { isLoading: true, loadingMessage: 'Extracting from 3 images...' }
    })

    expect(wrapper.text()).toContain('Extracting from 3 images...')
    expect(wrapper.find('.upload-area').exists()).toBe(false)
  })

  it('has appropriate ARIA labels on file input', () => {
    const wrapper = mount(ImageUpload)
    const input = wrapper.find('input[type="file"]')
    expect(input.attributes('aria-label')).toBeTruthy()
  })

  it('revokes all object URLs on unmount', async () => {
    const wrapper = mount(ImageUpload)

    await addFilesViaInput(wrapper, [createFile('a.jpg'), createFile('b.jpg')])

    vi.mocked(global.URL.revokeObjectURL).mockClear()
    wrapper.unmount()

    expect(global.URL.revokeObjectURL).toHaveBeenCalledTimes(2)
  })
})
