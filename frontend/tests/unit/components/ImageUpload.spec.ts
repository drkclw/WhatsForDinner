import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import ImageUpload from '@/components/ImageUpload.vue'

// Mock URL.createObjectURL and revokeObjectURL for jsdom
beforeEach(() => {
  global.URL.createObjectURL = vi.fn(() => 'blob:mock-url')
  global.URL.revokeObjectURL = vi.fn()
})

describe('ImageUpload', () => {
  it('renders the file input', () => {
    const wrapper = mount(ImageUpload)
    expect(wrapper.find('input[type="file"]').exists()).toBe(true)
  })

  it('accepts only JPEG, PNG, and WebP files', () => {
    const wrapper = mount(ImageUpload)
    const input = wrapper.find('input[type="file"]')
    expect(input.attributes('accept')).toBe('image/jpeg,image/png,image/webp')
  })

  it('shows error for unsupported file type', async () => {
    const wrapper = mount(ImageUpload)
    
    const file = new File(['content'], 'document.pdf', { type: 'application/pdf' })
    const input = wrapper.find('input[type="file"]')
    
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')
    
    expect(wrapper.text()).toContain('Unsupported')
    expect(wrapper.emitted('file-selected')).toBeFalsy()
  })

  it('shows error for file exceeding 10 MB', async () => {
    const wrapper = mount(ImageUpload)
    
    const largeContent = new Uint8Array(11 * 1024 * 1024) // 11 MB
    const file = new File([largeContent], 'large-image.jpg', { type: 'image/jpeg' })
    const input = wrapper.find('input[type="file"]')
    
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')
    
    expect(wrapper.text()).toContain('10 MB')
    expect(wrapper.emitted('file-selected')).toBeFalsy()
  })

  it('emits file-selected event for valid file', async () => {
    const wrapper = mount(ImageUpload)
    
    const file = new File(['image-content'], 'recipe.jpg', { type: 'image/jpeg' })
    Object.defineProperty(file, 'size', { value: 1024 * 1024 }) // 1 MB
    const input = wrapper.find('input[type="file"]')
    
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')
    
    expect(wrapper.emitted('file-selected')).toBeTruthy()
    expect(wrapper.emitted('file-selected')![0][0]).toEqual(file)
  })

  it('has appropriate ARIA labels', () => {
    const wrapper = mount(ImageUpload)
    const input = wrapper.find('input[type="file"]')
    expect(input.attributes('aria-label')).toBeTruthy()
  })
})
