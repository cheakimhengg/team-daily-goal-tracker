<script setup lang="ts">
import { Mood } from '../types/Mood'

const props = defineProps<{
  currentMood: Mood | null
}>()

const emit = defineEmits<{
  'mood-changed': [mood: Mood]
}>()

const moods = [
  { value: Mood.Great, label: 'Great', emoji: '😊' },
  { value: Mood.Good, label: 'Good', emoji: '🙂' },
  { value: Mood.Okay, label: 'Okay', emoji: '😐' },
  { value: Mood.Struggling, label: 'Struggling', emoji: '😟' },
  { value: Mood.Overwhelmed, label: 'Overwhelmed', emoji: '😰' }
]

const handleMoodClick = (mood: Mood) => {
  emit('mood-changed', mood)
}
</script>

<template>
  <div class="mood-selector">
    <label class="text-sm font-medium mb-2 block">How are you feeling today?</label>
    <div class="btn-group btn-group-horizontal flex flex-wrap gap-2">
      <button
        v-for="mood in moods"
        :key="mood.value"
        @click="handleMoodClick(mood.value)"
        class="btn btn-sm"
        :class="{
          'btn-primary': currentMood === mood.value,
          'btn-outline': currentMood !== mood.value
        }"
        :title="mood.label"
      >
        <span class="text-lg">{{ mood.emoji }}</span>
        <span class="hidden sm:inline ml-1">{{ mood.label }}</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.mood-selector {
  margin-top: 1rem;
}
</style>
