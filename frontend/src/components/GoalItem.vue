<script setup lang="ts">
import type { Goal } from '../types/Goal'

const props = defineProps<{
  goal: Goal
  canEdit: boolean
}>()

const emit = defineEmits<{
  'goal-toggled': [goalId: number]
  'goal-deleted': [goalId: number]
}>()

const handleToggle = () => {
  if (props.canEdit) {
    emit('goal-toggled', props.goal.id)
  }
}

const handleDelete = () => {
  if (props.canEdit && confirm('Are you sure you want to delete this goal?')) {
    emit('goal-deleted', props.goal.id)
  }
}
</script>

<template>
  <div class="flex items-center gap-2 py-2">
    <input
      v-if="canEdit"
      type="checkbox"
      :checked="goal.isCompleted"
      @change="handleToggle"
      class="checkbox checkbox-sm"
    />
    <span
      :class="{
        'line-through text-gray-400': goal.isCompleted,
        'flex-1': true
      }"
    >
      {{ goal.goalText }}
    </span>
    <button
      v-if="canEdit"
      @click="handleDelete"
      class="btn btn-ghost btn-xs text-error"
      title="Delete goal"
    >
      ✕
    </button>
  </div>
</template>
