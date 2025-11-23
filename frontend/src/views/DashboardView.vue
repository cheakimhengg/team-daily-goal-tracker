<script setup lang="ts">
import { ref, onMounted } from 'vue'
import IdentitySelector from '../components/IdentitySelector.vue'
import TeamMemberCard from '../components/TeamMemberCard.vue'
import { getTeamMembers, createGoal, toggleGoalCompletion, deleteGoal } from '../services/api'
import { useIdentity } from '../composables/useIdentity'
import type { TeamMember } from '../types/TeamMember'
import type { Goal } from '../types/Goal'

const { currentUserId } = useIdentity()
const teamMembers = ref<TeamMember[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    teamMembers.value = await getTeamMembers(true)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load team members'
  } finally {
    loading.value = false
  }
})

const handleIdentitySelected = (teamMemberId: number) => {
  currentUserId.value = teamMemberId
}

const handleGoalCreated = async (teamMemberId: number, goalText: string) => {
  const teamMember = teamMembers.value.find(tm => tm.id === teamMemberId)
  if (!teamMember) return

  try {
    const newGoal = await createGoal(teamMemberId, goalText)
    teamMember.goals.unshift(newGoal)
  } catch (e) {
    alert(e instanceof Error ? e.message : 'Failed to create goal')
  }
}

const handleGoalToggled = async (goalId: number) => {
  // Find the goal and team member
  let targetGoal: Goal | undefined
  let targetTeamMember: TeamMember | undefined

  for (const tm of teamMembers.value) {
    const goal = tm.goals.find(g => g.id === goalId)
    if (goal) {
      targetGoal = goal
      targetTeamMember = tm
      break
    }
  }

  if (!targetGoal || !targetTeamMember) return

  // Optimistic update
  const previousState = targetGoal.isCompleted
  targetGoal.isCompleted = !targetGoal.isCompleted

  try {
    await toggleGoalCompletion(goalId)
  } catch (e) {
    // Rollback on error
    targetGoal.isCompleted = previousState
    alert(e instanceof Error ? e.message : 'Failed to toggle goal')
  }
}

const handleGoalDeleted = async (goalId: number) => {
  // Find the team member with this goal
  let targetTeamMember: TeamMember | undefined
  let goalIndex = -1

  for (const tm of teamMembers.value) {
    goalIndex = tm.goals.findIndex(g => g.id === goalId)
    if (goalIndex !== -1) {
      targetTeamMember = tm
      break
    }
  }

  if (!targetTeamMember || goalIndex === -1) return

  // Optimistic update
  const deletedGoal = targetTeamMember.goals.splice(goalIndex, 1)[0]

  try {
    await deleteGoal(goalId)
  } catch (e) {
    // Rollback on error
    targetTeamMember.goals.splice(goalIndex, 0, deletedGoal)
    alert(e instanceof Error ? e.message : 'Failed to delete goal')
  }
}
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-8">Team Daily Goal Tracker</h1>

    <div v-if="loading" class="text-center">
      <span class="loading loading-spinner loading-lg"></span>
    </div>

    <div v-else-if="error" class="alert alert-error">
      <span>{{ error }}</span>
    </div>

    <div v-else>
      <IdentitySelector
        :team-members="teamMembers"
        @identity-selected="handleIdentitySelected"
      />

      <div v-if="currentUserId" class="mt-6 mb-4">
        <p class="text-lg">
          Welcome,
          <span class="font-semibold">
            {{ teamMembers.find(tm => tm.id === currentUserId)?.name }}
          </span>!
        </p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-8">
        <TeamMemberCard
          v-for="teamMember in teamMembers"
          :key="teamMember.id"
          :team-member="teamMember"
          :is-current-user="teamMember.id === currentUserId"
          @goal-created="handleGoalCreated"
          @goal-toggled="handleGoalToggled"
          @goal-deleted="handleGoalDeleted"
        />
      </div>
    </div>
  </div>
</template>
