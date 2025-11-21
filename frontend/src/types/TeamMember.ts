import type { Mood } from './Mood'
import type { Goal } from './Goal'

export interface TeamMember {
  id: number
  name: string
  currentMood: Mood | null
  moodUpdatedAt: string | null
  goals: Goal[]
}
