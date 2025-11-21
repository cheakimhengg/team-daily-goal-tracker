import type { TeamMember } from '../types/TeamMember'
import type { TeamMembersResponse } from '../types/ApiResponses'

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

export { fetchJSON, API_BASE_URL }
