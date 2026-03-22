import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import RecipeForm from '@/components/RecipeForm.vue'

describe('RecipeForm', () => {
  it('renders all form fields', () => {
    const wrapper = mount(RecipeForm)
    
    expect(wrapper.find('#name').exists()).toBe(true)
    expect(wrapper.find('#description').exists()).toBe(true)
    expect(wrapper.find('#ingredients').exists()).toBe(true)
    expect(wrapper.find('#cookTime').exists()).toBe(true)
  })

  it('shows validation error when name is empty on submit', async () => {
    const wrapper = mount(RecipeForm)
    
    await wrapper.find('form').trigger('submit.prevent')
    
    expect(wrapper.find('#name-error').exists()).toBe(true)
    expect(wrapper.find('#name-error').text()).toContain('required')
  })

  it('emits submit event with form data when valid', async () => {
    const wrapper = mount(RecipeForm)
    
    await wrapper.find('#name').setValue('Test Recipe')
    await wrapper.find('#description').setValue('A test description')
    await wrapper.find('#ingredients').setValue('Flour\nSugar')
    await wrapper.find('#cookTime').setValue(30)
    
    await wrapper.find('form').trigger('submit.prevent')
    
    expect(wrapper.emitted('submit')).toBeTruthy()
    const emittedData = wrapper.emitted('submit')![0][0] as Record<string, unknown>
    expect(emittedData.name).toBe('Test Recipe')
    expect(emittedData.description).toBe('A test description')
    expect(emittedData.ingredients).toBe('Flour\nSugar')
    expect(emittedData.cookTimeMinutes).toBe(30)
  })

  it('emits cancel event when cancel button is clicked', async () => {
    const wrapper = mount(RecipeForm)
    
    await wrapper.find('[data-testid="cancel-btn"]').trigger('click')
    
    expect(wrapper.emitted('cancel')).toBeTruthy()
  })

  it('pre-populates form with initialData prop', () => {
    const initialData = {
      name: 'Existing Recipe',
      description: 'Existing description',
      ingredients: 'Existing ingredients',
      cookTimeMinutes: 60
    }
    
    const wrapper = mount(RecipeForm, {
      props: { initialData }
    })
    
    expect((wrapper.find('#name').element as HTMLInputElement).value).toBe('Existing Recipe')
    expect((wrapper.find('#description').element as HTMLTextAreaElement).value).toBe('Existing description')
    expect((wrapper.find('#ingredients').element as HTMLTextAreaElement).value).toBe('Existing ingredients')
    expect((wrapper.find('#cookTime').element as HTMLInputElement).value).toBe('60')
  })
})
