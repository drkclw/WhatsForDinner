import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

describe('ConfirmDialog.vue', () => {
  const defaultProps = {
    show: true,
    title: 'Delete Recipe',
    message: 'Are you sure you want to delete this recipe?'
  }

  const mountOptions = {
    props: defaultProps,
    global: {
      stubs: {
        teleport: true
      }
    }
  }

  it('renders title and message when shown', () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    expect(wrapper.text()).toContain('Delete Recipe')
    expect(wrapper.text()).toContain('Are you sure you want to delete this recipe?')
  })

  it('does not render content when show is false', () => {
    const wrapper = mount(ConfirmDialog, {
      ...mountOptions,
      props: { ...defaultProps, show: false }
    })
    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)
  })

  it('emits confirm event when confirm button is clicked', async () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    await wrapper.find('[data-testid="confirm-btn"]').trigger('click')
    expect(wrapper.emitted('confirm')).toHaveLength(1)
  })

  it('emits cancel event when cancel button is clicked', async () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    await wrapper.find('[data-testid="cancel-btn"]').trigger('click')
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('emits cancel event on Escape key press', async () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    await wrapper.find('[role="dialog"]').trigger('keydown', { key: 'Escape' })
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('emits confirm event on Enter key press', async () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    await wrapper.find('[role="dialog"]').trigger('keydown', { key: 'Enter' })
    expect(wrapper.emitted('confirm')).toHaveLength(1)
  })

  it('has ARIA role="dialog" and aria-modal', () => {
    const wrapper = mount(ConfirmDialog, mountOptions)
    const dialog = wrapper.find('[role="dialog"]')
    expect(dialog.exists()).toBe(true)
    expect(dialog.attributes('aria-modal')).toBe('true')
  })

  it('uses custom confirm and cancel labels', () => {
    const wrapper = mount(ConfirmDialog, {
      ...mountOptions,
      props: {
        ...defaultProps,
        confirmLabel: 'Yes, Delete',
        cancelLabel: 'No, Keep'
      }
    })
    expect(wrapper.find('[data-testid="confirm-btn"]').text()).toBe('Yes, Delete')
    expect(wrapper.find('[data-testid="cancel-btn"]').text()).toBe('No, Keep')
  })
})
