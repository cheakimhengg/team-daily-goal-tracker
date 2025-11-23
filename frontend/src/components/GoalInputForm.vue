<script setup lang="ts">
import { ref, computed } from 'vue'

const emit = defineEmits<{
  'goal-created': [goalText: string]
  'cancel': []
}>()

const goalText = ref('')
const MAX_LENGTH = 500

const characterCount = computed(() => goalText.value.length)
const isValid = computed(() => goalText.value.trim().length > 0 && goalText.value.length <= MAX_LENGTH)
const characterCountClass = computed(() => {
  if (characterCount.value > MAX_LENGTH) return 'text-error'
  if (characterCount.value > MAX_LENGTH * 0.9) return 'text-warning'
  return 'text-gray-500'
})

const handleSubmit = () => {
  if (isValid.value) {
    emit('goal-created', goalText.value.trim())
    goalText.value = ''
  }
}

const handleCancel = () => {
  goalText.value = ''
  emit('cancel')
}
</script>

<template>
  <div class="card bg-base-200 p-4">
    <div class="form-control">
      <label class="label">
        <span class="label-text font-semibold">New Goal</span>
        <span class="label-text-alt" :class="characterCountClass">
          {{ characterCount }}/{{ MAX_LENGTH }}
        </span>
      </label>
      <textarea
        v-model="goalText"
        class="textarea textarea-bordered h-24"
        :class="{ 'textarea-error': characterCount > MAX_LENGTH }"
        placeholder="What do you want to accomplish today?"
        :maxlength="MAX_LENGTH + 50"
      ></textarea>
      <div v-if="goalText.trim().length === 0 && goalText.length > 0" class="label">
        <span class="label-text-alt text-error">Goal text cannot be empty</span>
      </div>
      <div v-if="characterCount > MAX_LENGTH" class="label">
        <span class="label-text-alt text-error">Goal text is too long</span>
      </div>
    </div>
    <div class="card-actions justify-end mt-2">
      <button @click="handleCancel" class="btn btn-ghost btn-sm">
        Cancel
      </button>
      <button
        @click="handleSubmit"
        class="btn btn-primary btn-sm"
        :disabled="!isValid"
      >
        Add Goal
      </button>
    </div>
  </div>
</template>
