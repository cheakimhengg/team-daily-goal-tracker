import type { TeamMember } from './TeamMember'
import type { Goal } from './Goal'

export interface ApiResponse<T> {
  data: T
}

export interface ApiError {
  error: {
    code: string
    message: string
    details?: Record<string, string[]>
  }
}

export type TeamMembersResponse = ApiResponse<TeamMember[]>
export type TeamMemberResponse = ApiResponse<TeamMember>
export type GoalResponse = ApiResponse<Goal>
export type GoalsResponse = ApiResponse<Goal[]>

export interface Stats {
  date: string
  totalGoals: number
  completedGoals: number
  completionRate: number
  moodBreakdown: Record<string, number>
  teamSize: number
}

export type StatsResponse = ApiResponse<Stats>
