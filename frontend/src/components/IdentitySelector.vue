<script setup lang="ts">
import { ref } from 'vue'
import type { TeamMember } from '../types/TeamMember'

const props = defineProps<{
  teamMembers: TeamMember[]
}>()

const emit = defineEmits<{
  'identity-selected': [teamMemberId: number]
}>()

const selectedId = ref<number | null>(null)

const handleSelect = () => {
  if (selectedId.value) {
    emit('identity-selected', selectedId.value)
  }
}
</script>

<template>
  <div class="flex items-center gap-2 mb-6">
    <label for="identity-selector" class="text-lg font-medium">I am:</label>
    <select
      id="identity-selector"
      v-model="selectedId"
      @change="handleSelect"
      class="select select-bordered"
    >
      <option :value="null" disabled selected>Select your name...</option>
      <option v-for="tm in teamMembers" :key="tm.id" :value="tm.id">
        {{ tm.name }}
      </option>
    </select>
  </div>
</template>
