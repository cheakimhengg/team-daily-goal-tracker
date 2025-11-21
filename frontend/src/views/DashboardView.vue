<script setup lang="ts">
import { ref, onMounted } from 'vue'
import IdentitySelector from '../components/IdentitySelector.vue'
import { getTeamMembers } from '../services/api'
import { useIdentity } from '../composables/useIdentity'
import type { TeamMember } from '../types/TeamMember'

const { currentUserId } = useIdentity()
const teamMembers = ref<TeamMember[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    teamMembers.value = await getTeamMembers(false)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load team members'
  } finally {
    loading.value = false
  }
})

const handleIdentitySelected = (teamMemberId: number) => {
  currentUserId.value = teamMemberId
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

      <div v-if="currentUserId" class="mt-6">
        <p class="text-lg">
          Welcome,
          <span class="font-semibold">
            {{ teamMembers.find(tm => tm.id === currentUserId)?.name }}
          </span>!
        </p>
      </div>
    </div>
  </div>
</template>
