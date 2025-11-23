<script setup lang="ts">
import { ref, computed } from 'vue'
import type { TeamMember } from '../types/TeamMember'
import { Mood } from '../types/Mood'
import GoalItem from './GoalItem.vue'
import GoalInputForm from './GoalInputForm.vue'
import MoodSelector from './MoodSelector.vue'

const props = defineProps<{
  teamMember: TeamMember
  isCurrentUser: boolean
}>()

const emit = defineEmits<{
  'goal-created': [teamMemberId: number, goalText: string]
  'goal-toggled': [goalId: number]
  'goal-deleted': [goalId: number]
  'mood-changed': [teamMemberId: number, mood: Mood]
}>()

const showGoalForm = ref(false)

const moodBackgroundClass = computed(() => {
  if (!props.teamMember.currentMood) return ''

  const moodMap: Record<Mood, string> = {
    [Mood.Great]: 'bg-success/10',
    [Mood.Good]: 'bg-info/10',
    [Mood.Okay]: 'bg-warning/10',
    [Mood.Struggling]: 'bg-orange-500/10',
    [Mood.Overwhelmed]: 'bg-error/10'
  }

  return moodMap[props.teamMember.currentMood] || ''
})

const moodBadgeClass = computed(() => {
  if (!props.teamMember.currentMood) return 'badge-ghost'

  const badgeMap: Record<Mood, string> = {
    [Mood.Great]: 'badge-success',
    [Mood.Good]: 'badge-info',
    [Mood.Okay]: 'badge-warning',
    [Mood.Struggling]: 'badge-warning',
    [Mood.Overwhelmed]: 'badge-error'
  }

  return badgeMap[props.teamMember.currentMood] || 'badge-ghost'
})

const handleAddGoalClick = () => {
  showGoalForm.value = true
}

const handleGoalCreated = (goalText: string) => {
  emit('goal-created', props.teamMember.id, goalText)
  showGoalForm.value = false
}

const handleCancelGoal = () => {
  showGoalForm.value = false
}

const handleGoalToggled = (goalId: number) => {
  emit('goal-toggled', goalId)
}

const handleGoalDeleted = (goalId: number) => {
  emit('goal-deleted', goalId)
}

const handleMoodChanged = (mood: Mood) => {
  emit('mood-changed', props.teamMember.id, mood)
}
</script>

<template>
  <div class="card bg-base-100 shadow-md" :class="moodBackgroundClass">
    <div class="card-body">
      <h2 class="card-title flex justify-between items-center">
        <span>{{ teamMember.name }}</span>
        <span
          v-if="teamMember.currentMood"
          class="badge"
          :class="moodBadgeClass"
        >
          {{ teamMember.currentMood }}
        </span>
      </h2>

      <div v-if="teamMember.goals && teamMember.goals.length > 0" class="mt-4">
        <h3 class="font-semibold mb-2">Goals:</h3>
        <div class="space-y-1">
          <GoalItem
            v-for="goal in teamMember.goals"
            :key="goal.id"
            :goal="goal"
            :can-edit="isCurrentUser"
            @goal-toggled="handleGoalToggled"
            @goal-deleted="handleGoalDeleted"
          />
        </div>
      </div>

      <div v-else-if="!isCurrentUser" class="text-gray-400 italic mt-4">
        No goals set yet
      </div>

      <MoodSelector
        v-if="isCurrentUser"
        :current-mood="teamMember.currentMood"
        @mood-changed="handleMoodChanged"
      />

      <div v-if="isCurrentUser" class="mt-4">
        <GoalInputForm
          v-if="showGoalForm"
          @goal-created="handleGoalCreated"
          @cancel="handleCancelGoal"
        />
        <button
          v-else
          @click="handleAddGoalClick"
          class="btn btn-primary btn-sm w-full"
        >
          + Add Goal
        </button>
      </div>
    </div>
  </div>
</template>
