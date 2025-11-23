import type { TeamMember } from '../types/TeamMember'
import type { Goal } from '../types/Goal'
import type { Mood } from '../types/Mood'
import type { TeamMembersResponse, GoalResponse, TeamMemberResponse } from '../types/ApiResponses'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

async function fetchJSON<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers
    }
  })

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({
      error: { code: 'UNKNOWN_ERROR', message: 'Unknown error occurred' }
    }))
    throw new Error(errorData.error?.message || `HTTP ${response.status}`)
  }

  if (response.status === 204) {
    return null as T
  }

  return response.json()
}

// API functions
export async function getTeamMembers(includeGoals: boolean = false): Promise<TeamMember[]> {
  const response = await fetchJSON<TeamMembersResponse>(
    `/api/team-members?includeGoals=${includeGoals}`
  )
  return response.data
}

export async function createGoal(teamMemberId: number, goalText: string): Promise<Goal> {
  const response = await fetchJSON<GoalResponse>('/api/goals', {
    method: 'POST',
    body: JSON.stringify({ teamMemberId, goalText })
  })
  return response.data
}

export async function toggleGoalCompletion(goalId: number): Promise<Goal> {
  const response = await fetchJSON<GoalResponse>(`/api/goals/${goalId}/toggle`, {
    method: 'PUT'
  })
  return response.data
}

export async function deleteGoal(goalId: number): Promise<void> {
  await fetchJSON<void>(`/api/goals/${goalId}`, {
    method: 'DELETE'
  })
}

export async function updateMood(teamMemberId: number, mood: Mood): Promise<TeamMember> {
  const response = await fetchJSON<TeamMemberResponse>(`/api/team-members/${teamMemberId}/mood`, {
    method: 'PUT',
    body: JSON.stringify({ mood })
  })
  return response.data
}

export { fetchJSON, API_BASE_URL }
