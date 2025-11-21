import { ref, watch, onMounted } from 'vue'

const currentUserId = ref<number | null>(null)

export function useIdentity() {
  // Load from session storage on mount
  onMounted(() => {
    const stored = sessionStorage.getItem('currentUserId')
    if (stored) {
      currentUserId.value = parseInt(stored, 10)
    }
  })

  // Save to session storage on change
  watch(currentUserId, (newId) => {
    if (newId) {
      sessionStorage.setItem('currentUserId', newId.toString())
    } else {
      sessionStorage.removeItem('currentUserId')
    }
  })

  return { currentUserId }
}
