# Implementation Plan: Team Daily Goal Tracker with Mood Sync

**Feature**: [spec.md](spec.md)
**Created**: 2025-11-20
**Status**: Ready for Implementation

---

## Executive Summary

This plan details the implementation of a Team Daily Goal Tracker with Mood Sync - a desktop web application for small teams (≤20 members) to track daily goals and team moods in real-time. The MVP focuses on 4 core user stories (US0-US3) with an optional P2 statistics feature (US4).

**Timeline Estimate**: 3-5 days for P1 features (US0-US3)
**Team Size**: 1-2 developers
**Tech Stack**: Vue 3 + TypeScript + DaisyUI (frontend), .NET 8 + Dapper + SQLite (backend)

---

## Constitution Alignment ✅

### Technical Constraints (Non-Negotiable)

| Constraint | Implementation |
|-----------|----------------|
| Frontend Framework | Vue 3 with Composition API (`<script setup>` syntax) |
| Frontend Language | TypeScript with strict mode enabled |
| UI Library | DaisyUI (Tailwind CSS-based components) |
| Build Tool | Vite |
| Backend Framework | .NET 8 Web API (ASP.NET Core) |
| Backend Language | C# 12 |
| ORM | Dapper only (no Entity Framework) |
| Database | SQLite (file-based: `backend/Data/team-tracker.db`) |
| Target Platform | Desktop web browsers only (Chrome, Firefox, Safari latest 2 versions) |

**Prohibited**:
- ❌ React, Angular, Svelte
- ❌ Vue Options API
- ❌ Entity Framework or other ORMs
- ❌ PostgreSQL, MySQL, MSSQL
- ❌ Mobile responsive design

### Core Principles Alignment

1. **Code Quality First**: Production-grade code from day one, strict TypeScript, comprehensive error handling
2. **Clear & Intuitive UX**: Max 3 clicks for primary actions, DaisyUI components for consistency
3. **Minimal Architecture**: 2 tables, 6 API endpoints, no unnecessary abstractions
4. **Performance**: < 2s page load, < 200ms API p95 latency
5. **Strict MVP Scoping**: P1 features only (US0-US3), explicitly exclude 40+ out-of-scope features
6. **Fast Iteration**: Feature implementation in 1-2 day increments

---

## High-Level Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Browser (Desktop)                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   Vue 3 SPA (http://localhost:5173)                   │  │
│  │   ┌──────────────────────────────────────────────┐    │  │
│  │   │  DashboardView.vue                           │    │  │
│  │   │  ├─ IdentitySelector.vue (US0)               │    │  │
│  │   │  ├─ TeamMemberCard.vue (US1)                 │    │  │
│  │   │  │  ├─ GoalItem.vue                          │    │  │
│  │   │  │  ├─ GoalInputForm.vue (US2)               │    │  │
│  │   │  │  └─ MoodSelector.vue (US3)                │    │  │
│  │   │  └─ StatsPanel.vue (US4 - P2)                │    │  │
│  │   └──────────────────────────────────────────────┘    │  │
│  │                       ↕ HTTP/JSON                      │  │
│  │   services/api.ts (fetch wrapper)                     │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↕ REST API
┌─────────────────────────────────────────────────────────────┐
│        .NET 8 Web API (http://localhost:5000)               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Controllers (HTTP endpoints)                         │  │
│  │  ├─ TeamMembersController: GET /api/team-members     │  │
│  │  │                         PUT /api/team-members/:id/mood
│  │  ├─ GoalsController: GET, POST, PUT, DELETE /api/goals│  │
│  │  └─ StatsController: GET /api/stats (P2)             │  │
│  └───────────────────────────────────────────────────────┘  │
│                           ↕                                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Services (Business Logic)                            │  │
│  │  ├─ TeamMemberService                                 │  │
│  │  ├─ GoalService                                       │  │
│  │  └─ StatsService (P2)                                 │  │
│  └───────────────────────────────────────────────────────┘  │
│                           ↕                                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Repositories (Data Access with Dapper)               │  │
│  │  ├─ TeamMemberRepository                              │  │
│  │  └─ GoalRepository                                    │  │
│  └───────────────────────────────────────────────────────┘  │
│                           ↕                                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  SQLite Database (backend/Data/team-tracker.db)       │  │
│  │  ├─ TeamMembers (Id, Name, CurrentMood, MoodUpdatedAt)│  │
│  │  └─ Goals (Id, TeamMemberId, GoalText, CreatedAt, IsCompleted)
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Key Design Decisions

1. **Monorepo Structure**: `/frontend` and `/backend` directories in single repository
2. **Stateless API**: No session state in backend (identity stored client-side only)
3. **Optimistic UI Updates**: Frontend updates immediately, rollback on API error
4. **Normalized Database**: 3NF schema with foreign key constraints
5. **No Authentication**: Per MVP scope, anyone can update any team member's data

---

## Frontend Architecture (Vue 3 + TypeScript + DaisyUI)

### Project Structure

```
frontend/
├── src/
│   ├── components/               # Reusable components
│   │   ├── IdentitySelector.vue  # US0: Dropdown to select team member
│   │   ├── TeamMemberCard.vue    # US1: Display one team member's info
│   │   ├── GoalItem.vue          # US1: Single goal display with checkbox
│   │   ├── GoalInputForm.vue     # US2: Form to add new goal
│   │   ├── MoodSelector.vue      # US3: Mood button group
│   │   └── StatsPanel.vue        # US4: Daily statistics (P2)
│   │
│   ├── views/
│   │   └── DashboardView.vue     # Main page - composes all components
│   │
│   ├── services/
│   │   └── api.ts                # API client (fetch wrapper)
│   │
│   ├── types/
│   │   ├── TeamMember.ts         # TypeScript interfaces
│   │   ├── Goal.ts
│   │   ├── Mood.ts
│   │   └── ApiResponses.ts
│   │
│   ├── composables/
│   │   └── useIdentity.ts        # Session identity management
│   │
│   ├── App.vue                   # Root component
│   └── main.ts                   # Entry point (Vue app initialization)
│
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
└── .env                          # VITE_API_BASE_URL=http://localhost:5000
```

### Component Breakdown

#### 1. DashboardView.vue (Main Page)
**Responsibility**: Orchestrate all child components, manage dashboard state

**State**:
- `teamMembers: Ref<TeamMember[]>` - All team members with goals
- `currentUserId: Ref<number | null>` - Selected identity (from useIdentity composable)
- `isLoading: Ref<boolean>` - Loading state during API calls

**Lifecycle**:
1. `onMounted`: Fetch team members with goals (`GET /api/team-members?includeGoals=true`)
2. Render `IdentitySelector` (pass teamMembers as prop)
3. Render `TeamMemberCard` for each team member (v-for)
4. Render `StatsPanel` (P2)

**Props**: None (root view)
**Emits**: None

---

#### 2. IdentitySelector.vue (US0)
**Responsibility**: Dropdown for user to select their identity

**Props**:
- `teamMembers: TeamMember[]` - List of available team members

**Emits**:
- `identity-selected: (teamMemberId: number) => void`

**State**:
- `selectedId: Ref<number | null>` - Currently selected team member ID

**Template** (DaisyUI):
```vue
<select class="select select-bordered" v-model="selectedId" @change="handleSelect">
  <option disabled selected value="">I am...</option>
  <option v-for="tm in teamMembers" :key="tm.id" :value="tm.id">
    {{ tm.name }}
  </option>
</select>
```

**Behavior**:
- On selection, store `selectedId` in session storage (via `useIdentity` composable)
- Emit `identity-selected` event with `teamMemberId`

---

#### 3. TeamMemberCard.vue (US1)
**Responsibility**: Display one team member's info (name, mood, goals)

**Props**:
- `teamMember: TeamMember` - Team member data
- `isCurrentUser: boolean` - True if this is the logged-in user's card

**Emits**:
- `goal-added: (goal: Goal) => void`
- `goal-updated: (goal: Goal) => void`
- `goal-deleted: (goalId: number) => void`
- `mood-updated: (teamMember: TeamMember) => void`

**State**:
- `showGoalForm: Ref<boolean>` - Toggle goal input form

**Template** (DaisyUI):
```vue
<div class="card bg-base-100 shadow-xl" :class="moodBackgroundClass">
  <div class="card-body">
    <h2 class="card-title">{{ teamMember.name }}</h2>

    <MoodSelector
      v-if="isCurrentUser"
      :current-mood="teamMember.currentMood"
      @mood-changed="handleMoodChange"
    />

    <div v-else class="badge" :class="moodBadgeClass">
      {{ teamMember.currentMood || 'No mood set' }}
    </div>

    <div class="divider"></div>

    <GoalItem
      v-for="goal in teamMember.goals"
      :key="goal.id"
      :goal="goal"
      :can-edit="isCurrentUser"
      @goal-toggled="handleGoalToggle"
      @goal-deleted="handleGoalDelete"
    />

    <button v-if="isCurrentUser" @click="showGoalForm = true" class="btn btn-primary btn-sm">
      + Add Goal
    </button>

    <GoalInputForm
      v-if="showGoalForm"
      :team-member-id="teamMember.id"
      @goal-created="handleGoalCreated"
      @cancel="showGoalForm = false"
    />
  </div>
</div>
```

**Computed**:
- `moodBackgroundClass`: Map mood to DaisyUI color (e.g., `bg-success/10` for "Great")
- `moodBadgeClass`: Map mood to badge color

---

#### 4. GoalItem.vue (US1, US2)
**Responsibility**: Display single goal with checkbox and delete button

**Props**:
- `goal: Goal`
- `canEdit: boolean` - If true, show checkbox and delete button

**Emits**:
- `goal-toggled: (goalId: number) => void`
- `goal-deleted: (goalId: number) => void`

**Template** (DaisyUI):
```vue
<div class="flex items-center gap-2">
  <input
    v-if="canEdit"
    type="checkbox"
    :checked="goal.isCompleted"
    @change="$emit('goal-toggled', goal.id)"
    class="checkbox checkbox-primary"
  />
  <span :class="{ 'line-through text-gray-400': goal.isCompleted }">
    {{ goal.goalText }}
  </span>
  <button
    v-if="canEdit"
    @click="confirmDelete"
    class="btn btn-error btn-xs ml-auto"
  >
    🗑️
  </button>
</div>
```

**Methods**:
- `confirmDelete()`: Show browser confirm dialog before emitting `goal-deleted`

---

#### 5. GoalInputForm.vue (US2)
**Responsibility**: Form to add new goal

**Props**:
- `teamMemberId: number`

**Emits**:
- `goal-created: (goal: Goal) => void`
- `cancel: () => void`

**State**:
- `goalText: Ref<string>` - User input
- `errorMessage: Ref<string>` - Validation error
- `isSubmitting: Ref<boolean>` - Loading state

**Template** (DaisyUI):
```vue
<form @submit.prevent="handleSubmit" class="mt-4">
  <textarea
    v-model="goalText"
    placeholder="What do you want to accomplish today?"
    maxlength="500"
    class="textarea textarea-bordered w-full"
  ></textarea>
  <p class="text-sm text-gray-500">{{ goalText.length }}/500</p>
  <p v-if="errorMessage" class="text-error text-sm">{{ errorMessage }}</p>
  <div class="flex gap-2 mt-2">
    <button type="submit" :disabled="isSubmitting" class="btn btn-primary btn-sm">
      {{ isSubmitting ? 'Adding...' : 'Add Goal' }}
    </button>
    <button type="button" @click="$emit('cancel')" class="btn btn-ghost btn-sm">
      Cancel
    </button>
  </div>
</form>
```

**Validation**:
- Client-side: Check `goalText` length (1-500 chars)
- Trim whitespace before submit

---

#### 6. MoodSelector.vue (US3)
**Responsibility**: Button group to select mood

**Props**:
- `currentMood: Mood | null`

**Emits**:
- `mood-changed: (mood: Mood) => void`

**Template** (DaisyUI):
```vue
<div class="btn-group">
  <button
    v-for="mood in allMoods"
    :key="mood"
    @click="$emit('mood-changed', mood)"
    :class="[
      'btn btn-sm',
      currentMood === mood ? 'btn-active' : ''
    ]"
  >
    {{ getMoodEmoji(mood) }} {{ mood }}
  </button>
</div>
```

**Data**:
- `allMoods`: `['Great', 'Good', 'Okay', 'Struggling', 'Overwhelmed']`

**Methods**:
- `getMoodEmoji(mood: Mood): string` - Map mood to emoji (😊, 🙂, 😐, 😟, 😰)

---

#### 7. StatsPanel.vue (US4 - P2)
**Responsibility**: Display daily statistics

**State**:
- `stats: Ref<Stats | null>` - Statistics data

**Lifecycle**:
- `onMounted`: Fetch stats (`GET /api/stats`)

**Template** (DaisyUI):
```vue
<div class="stats shadow">
  <div class="stat">
    <div class="stat-title">Total Goals Today</div>
    <div class="stat-value">{{ stats?.totalGoals ?? 0 }}</div>
  </div>
  <div class="stat">
    <div class="stat-title">Completed</div>
    <div class="stat-value text-success">{{ stats?.completedGoals ?? 0 }}</div>
  </div>
  <div class="stat">
    <div class="stat-title">Completion Rate</div>
    <div class="stat-value">{{ stats?.completionRate.toFixed(1) ?? 0 }}%</div>
  </div>
</div>

<div class="mt-4">
  <h3>Team Mood Breakdown</h3>
  <div v-for="(count, mood) in stats?.moodBreakdown" :key="mood" class="badge">
    {{ mood }}: {{ count }}
  </div>
</div>
```

---

### State Management Strategy

**Approach**: No global store (Pinia not needed for MVP)

**Rationale**:
- Simple state scope (single dashboard page)
- Data fetched once on mount, updated via API calls
- Parent-child communication via props/emits is sufficient

**Identity Management**:
```typescript
// composables/useIdentity.ts
export function useIdentity() {
  const currentUserId = ref<number | null>(null);

  // Load from session storage on init
  onMounted(() => {
    const stored = sessionStorage.getItem('currentUserId');
    if (stored) {
      currentUserId.value = parseInt(stored, 10);
    }
  });

  // Save to session storage on change
  watch(currentUserId, (newId) => {
    if (newId) {
      sessionStorage.setItem('currentUserId', newId.toString());
    } else {
      sessionStorage.removeItem('currentUserId');
    }
  });

  return { currentUserId };
}
```

---

### API Service Layer

```typescript
// services/api.ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

async function fetchJSON<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers
    }
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ error: { message: 'Unknown error' } }));
    throw new Error(errorData.error?.message || `HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null as T; // No content
  }

  return response.json();
}

// API functions
export async function getTeamMembers(includeGoals: boolean = false): Promise<TeamMember[]> {
  const { data } = await fetchJSON<{ data: TeamMember[] }>(
    `/api/team-members?includeGoals=${includeGoals}`
  );
  return data;
}

export async function createGoal(teamMemberId: number, goalText: string): Promise<Goal> {
  const { data } = await fetchJSON<{ data: Goal }>('/api/goals', {
    method: 'POST',
    body: JSON.stringify({ teamMemberId, goalText: goalText.trim() })
  });
  return data;
}

export async function toggleGoalCompletion(goalId: number): Promise<Goal> {
  const { data } = await fetchJSON<{ data: Goal }>(`/api/goals/${goalId}/toggle`, {
    method: 'PUT',
    body: JSON.stringify({})
  });
  return data;
}

export async function deleteGoal(goalId: number): Promise<void> {
  await fetchJSON<void>(`/api/goals/${goalId}`, { method: 'DELETE' });
}

export async function updateMood(teamMemberId: number, mood: Mood): Promise<TeamMember> {
  const { data } = await fetchJSON<{ data: TeamMember }>(
    `/api/team-members/${teamMemberId}/mood`,
    {
      method: 'PUT',
      body: JSON.stringify({ mood })
    }
  );
  return data;
}

export async function getStats(date?: string): Promise<Stats> {
  const url = date ? `/api/stats?date=${date}` : '/api/stats';
  const { data } = await fetchJSON<{ data: Stats }>(url);
  return data;
}
```

---

## Backend Architecture (.NET 8 + Dapper + SQLite)

### Project Structure

```
backend/
├── Controllers/
│   ├── TeamMembersController.cs  # GET /api/team-members, PUT /:id/mood
│   ├── GoalsController.cs        # CRUD for goals
│   └── StatsController.cs        # GET /api/stats (P2)
│
├── Services/
│   ├── ITeamMemberService.cs     # Interface
│   ├── TeamMemberService.cs      # Business logic
│   ├── IGoalService.cs
│   ├── GoalService.cs
│   ├── IStatsService.cs          # P2
│   └── StatsService.cs           # P2
│
├── Data/
│   ├── IDbConnectionFactory.cs   # SQLite connection factory interface
│   ├── SqliteConnectionFactory.cs
│   ├── Repositories/
│   │   ├── ITeamMemberRepository.cs
│   │   ├── TeamMemberRepository.cs
│   │   ├── IGoalRepository.cs
│   │   └── GoalRepository.cs
│   ├── Migrations/
│   │   └── 001_InitialSchema.sql # Database initialization script
│   └── team-tracker.db           # SQLite database file (created at runtime)
│
├── Models/
│   ├── TeamMember.cs             # Entity model
│   ├── Goal.cs                   # Entity model
│   ├── Mood.cs                   # Enum
│   └── DTOs/
│       ├── GoalCreateRequest.cs
│       ├── GoalUpdateRequest.cs
│       ├── MoodUpdateRequest.cs
│       └── StatsResponse.cs      # P2
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Global error handling
│
├── Exceptions/
│   ├── TeamMemberNotFoundException.cs
│   └── GoalNotFoundException.cs
│
├── Program.cs                    # Entry point, DI configuration
├── appsettings.json              # Configuration
└── backend.csproj                # Project file
```

### Controller Layer

#### TeamMembersController.cs
```csharp
[ApiController]
[Route("api/team-members")]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;

    public TeamMembersController(ITeamMemberService teamMemberService)
    {
        _teamMemberService = teamMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeGoals = false)
    {
        var teamMembers = await _teamMemberService.GetAllAsync(includeGoals);
        return Ok(new { data = teamMembers });
    }

    [HttpPut("{id}/mood")]
    public async Task<IActionResult> UpdateMood(int id, [FromBody] MoodUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "One or more validation errors occurred",
                    details = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                }
            });
        }

        try
        {
            var teamMember = await _teamMemberService.UpdateMoodAsync(id, request.Mood);
            return Ok(new { data = teamMember });
        }
        catch (TeamMemberNotFoundException ex)
        {
            return NotFound(new { error = new { code = "TEAM_MEMBER_NOT_FOUND", message = ex.Message } });
        }
    }
}
```

#### GoalsController.cs
```csharp
[ApiController]
[Route("api/goals")]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoalCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { /* validation errors */ });
        }

        try
        {
            var goal = await _goalService.CreateAsync(request.TeamMemberId, request.GoalText);
            return CreatedAtAction(nameof(GetById), new { id = goal.Id }, new { data = goal });
        }
        catch (TeamMemberNotFoundException ex)
        {
            return NotFound(new { error = new { code = "TEAM_MEMBER_NOT_FOUND", message = ex.Message } });
        }
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleCompletion(int id)
    {
        try
        {
            var goal = await _goalService.ToggleCompletionAsync(id);
            return Ok(new { data = goal });
        }
        catch (GoalNotFoundException ex)
        {
            return NotFound(new { error = new { code = "GOAL_NOT_FOUND", message = ex.Message } });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _goalService.DeleteAsync(id);
            return NoContent();
        }
        catch (GoalNotFoundException ex)
        {
            return NotFound(new { error = new { code = "GOAL_NOT_FOUND", message = ex.Message } });
        }
    }
}
```

---

### Service Layer

#### TeamMemberService.cs
```csharp
public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamMemberRepository _teamMemberRepository;

    public TeamMemberService(ITeamMemberRepository teamMemberRepository)
    {
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<List<TeamMember>> GetAllAsync(bool includeGoals)
    {
        return await _teamMemberRepository.GetAllAsync(includeGoals);
    }

    public async Task<TeamMember> UpdateMoodAsync(int id, Mood mood)
    {
        var teamMember = await _teamMemberRepository.GetByIdAsync(id);
        if (teamMember == null)
        {
            throw new TeamMemberNotFoundException($"Team member with ID {id} does not exist");
        }

        teamMember.CurrentMood = mood;
        teamMember.MoodUpdatedAt = DateTime.UtcNow;

        await _teamMemberRepository.UpdateMoodAsync(id, mood, teamMember.MoodUpdatedAt.Value);

        return teamMember;
    }
}
```

#### GoalService.cs
```csharp
public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;

    public GoalService(IGoalRepository goalRepository, ITeamMemberRepository teamMemberRepository)
    {
        _goalRepository = goalRepository;
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<Goal> CreateAsync(int teamMemberId, string goalText)
    {
        // Validate team member exists
        var teamMember = await _teamMemberRepository.GetByIdAsync(teamMemberId);
        if (teamMember == null)
        {
            throw new TeamMemberNotFoundException($"Team member with ID {teamMemberId} does not exist");
        }

        var goal = new Goal
        {
            TeamMemberId = teamMemberId,
            GoalText = goalText,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        goal.Id = await _goalRepository.InsertAsync(goal);
        return goal;
    }

    public async Task<Goal> ToggleCompletionAsync(int goalId)
    {
        await _goalRepository.ToggleCompletionAsync(goalId);
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null)
        {
            throw new GoalNotFoundException($"Goal with ID {goalId} does not exist");
        }
        return goal;
    }

    public async Task DeleteAsync(int goalId)
    {
        var deleted = await _goalRepository.DeleteAsync(goalId);
        if (!deleted)
        {
            throw new GoalNotFoundException($"Goal with ID {goalId} does not exist");
        }
    }
}
```

---

### Repository Layer (Dapper Queries)

#### TeamMemberRepository.cs
```csharp
public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly IDbConnection _connection;

    public TeamMemberRepository(IDbConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
    }

    public async Task<List<TeamMember>> GetAllAsync(bool includeGoals)
    {
        if (!includeGoals)
        {
            var query = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers ORDER BY Name";
            var teamMembers = await _connection.QueryAsync<TeamMember>(query);
            return teamMembers.ToList();
        }

        // Multi-mapping query to include goals
        var query = @"
            SELECT
                tm.Id, tm.Name, tm.CurrentMood, tm.MoodUpdatedAt,
                g.Id, g.TeamMemberId, g.GoalText, g.CreatedAt, g.IsCompleted
            FROM TeamMembers tm
            LEFT JOIN Goals g ON tm.Id = g.TeamMemberId
            ORDER BY tm.Name, g.CreatedAt DESC
        ";

        var teamMemberDict = new Dictionary<int, TeamMember>();

        await _connection.QueryAsync<TeamMember, Goal, TeamMember>(
            query,
            (teamMember, goal) =>
            {
                if (!teamMemberDict.TryGetValue(teamMember.Id, out var existingTeamMember))
                {
                    existingTeamMember = teamMember;
                    existingTeamMember.Goals = new List<Goal>();
                    teamMemberDict.Add(teamMember.Id, existingTeamMember);
                }

                if (goal != null)
                {
                    existingTeamMember.Goals.Add(goal);
                }

                return existingTeamMember;
            },
            splitOn: "Id"
        );

        return teamMemberDict.Values.ToList();
    }

    public async Task<TeamMember?> GetByIdAsync(int id)
    {
        var query = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers WHERE Id = @Id";
        return await _connection.QuerySingleOrDefaultAsync<TeamMember>(query, new { Id = id });
    }

    public async Task UpdateMoodAsync(int id, Mood mood, DateTime timestamp)
    {
        var query = @"
            UPDATE TeamMembers
            SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp
            WHERE Id = @Id
        ";

        await _connection.ExecuteAsync(query, new
        {
            Id = id,
            Mood = mood.ToString(),
            Timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}
```

#### GoalRepository.cs
```csharp
public class GoalRepository : IGoalRepository
{
    private readonly IDbConnection _connection;

    public GoalRepository(IDbConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
    }

    public async Task<int> InsertAsync(Goal goal)
    {
        var query = @"
            INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
            VALUES (@TeamMemberId, @GoalText, @CreatedAt, 0);

            SELECT last_insert_rowid();
        ";

        var newId = await _connection.QuerySingleAsync<int>(query, new
        {
            goal.TeamMemberId,
            goal.GoalText,
            CreatedAt = goal.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });

        return newId;
    }

    public async Task<Goal?> GetByIdAsync(int id)
    {
        var query = "SELECT Id, TeamMemberId, GoalText, CreatedAt, IsCompleted FROM Goals WHERE Id = @Id";
        return await _connection.QuerySingleOrDefaultAsync<Goal>(query, new { Id = id });
    }

    public async Task ToggleCompletionAsync(int goalId)
    {
        var query = @"
            UPDATE Goals
            SET IsCompleted = CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END
            WHERE Id = @Id
        ";

        var rowsAffected = await _connection.ExecuteAsync(query, new { Id = goalId });
        if (rowsAffected == 0)
        {
            throw new GoalNotFoundException($"Goal with ID {goalId} does not exist");
        }
    }

    public async Task<bool> DeleteAsync(int goalId)
    {
        var query = "DELETE FROM Goals WHERE Id = @Id";
        var rowsAffected = await _connection.ExecuteAsync(query, new { Id = goalId });
        return rowsAffected > 0;
    }
}
```

---

### Dependency Injection Configuration

#### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();

// Repositories
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();

// Services
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IStatsService, StatsService>(); // P2

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Run database migrations
using (var scope = app.Services.CreateScope())
{
    var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    using var connection = connectionFactory.CreateConnection();
    connection.Open();

    var migrationScript = File.ReadAllText("Data/Migrations/001_InitialSchema.sql");
    connection.Execute(migrationScript);
}

app.Run();
```

---

## Database Schema (SQLite)

See [data-model.md](data-model.md) for complete schema documentation.

**Summary**:
- **TeamMembers**: Id, Name, CurrentMood, MoodUpdatedAt
- **Goals**: Id, TeamMemberId (FK), GoalText, CreatedAt, IsCompleted
- **Indexes**: `idx_goals_team_member`, `idx_goals_created_at`
- **Constraints**: CHECK constraints for mood enum, goal text length, foreign key with CASCADE DELETE

---

## API Endpoints

See [contracts/](contracts/) for detailed API documentation.

**Summary** (6 endpoints):

| Method | Endpoint | Purpose | User Story |
|--------|----------|---------|------------|
| GET | `/api/team-members?includeGoals={bool}` | Fetch all team members (optionally with goals) | US0, US1 |
| PUT | `/api/team-members/{id}/mood` | Update team member's mood | US3 |
| POST | `/api/goals` | Create new goal | US2 |
| PUT | `/api/goals/{id}/toggle` | Toggle goal completion | US2 |
| DELETE | `/api/goals/{id}` | Delete goal | US2 |
| GET | `/api/stats?date={YYYY-MM-DD}` | Get daily statistics | US4 (P2) |

---

## Implementation Sequence (By User Story)

### Phase 0: Setup (Foundation)
**Time Estimate**: 1-2 hours

1. ✅ Initialize Git repository
2. ✅ Create frontend project: `npm create vite@latest frontend -- --template vue-ts`
3. ✅ Create backend project: `dotnet new webapi -n backend`
4. ✅ Install frontend dependencies: Vue 3, DaisyUI, Tailwind CSS
5. ✅ Install backend dependencies: Dapper, Microsoft.Data.Sqlite
6. ✅ Configure Vite, Tailwind, DaisyUI
7. ✅ Create database migration script (`001_InitialSchema.sql`)
8. ✅ Setup CORS in backend `Program.cs`
9. ✅ Test "Hello World" on both frontend and backend

---

### Phase 1: User Story 0 - Identify Self (P1)
**Time Estimate**: 2-3 hours

**Backend Tasks**:
1. Create `TeamMember` model
2. Create `IDbConnectionFactory` and `SqliteConnectionFactory`
3. Create `ITeamMemberRepository` and `TeamMemberRepository`
   - Implement `GetAllAsync(includeGoals = false)`
4. Create `ITeamMemberService` and `TeamMemberService`
5. Create `TeamMembersController` with `GET /api/team-members` endpoint
6. Run migration script to create database and seed team members
7. Test endpoint with cURL or Postman

**Frontend Tasks**:
1. Create TypeScript types: `TeamMember.ts`, `Mood.ts`, `Goal.ts`
2. Create `services/api.ts` with `getTeamMembers()` function
3. Create `composables/useIdentity.ts` for session identity management
4. Create `IdentitySelector.vue` component
5. Create `DashboardView.vue` and integrate `IdentitySelector`
6. Test identity selection (should store in session storage)

**Acceptance Criteria**:
- [ ] Dropdown shows all team members
- [ ] Selecting identity stores `teamMemberId` in session storage
- [ ] Page refresh clears identity (user must re-select)

---

### Phase 2: User Story 1 - View Dashboard (P1)
**Time Estimate**: 3-4 hours

**Backend Tasks**:
1. Create `Goal` model
2. Update `TeamMemberRepository.GetAllAsync(includeGoals = true)` with Dapper multi-mapping
3. Create `IGoalRepository` interface
4. Test endpoint: `GET /api/team-members?includeGoals=true`

**Frontend Tasks**:
1. Update `api.ts` to pass `includeGoals=true` parameter
2. Create `TeamMemberCard.vue` component
   - Props: `teamMember`, `isCurrentUser`
   - Display name, mood, goals list
   - Conditional rendering: Show edit controls only if `isCurrentUser=true`
3. Create `GoalItem.vue` component
   - Display goal text
   - Show checkbox if `canEdit=true`
   - Show completion status (strikethrough if completed)
4. Update `DashboardView.vue`:
   - Fetch team members with goals on mount
   - Render `TeamMemberCard` for each team member (v-for)
5. Test dashboard display with seed data

**Acceptance Criteria**:
- [ ] Dashboard shows all team members in alphabetical order
- [ ] Each card shows name, current mood (or "No mood set"), and list of goals
- [ ] Goals display in reverse chronological order (newest first)
- [ ] Completed goals show strikethrough styling

---

### Phase 3: User Story 2 - Set Goals (P1)
**Time Estimate**: 4-5 hours

**Backend Tasks**:
1. Create DTOs: `GoalCreateRequest.cs`, `GoalUpdateRequest.cs`
2. Implement `GoalRepository`:
   - `InsertAsync(goal)` - Returns new goal ID
   - `GetByIdAsync(id)`
   - `ToggleCompletionAsync(id)`
   - `DeleteAsync(id)`
3. Create `IGoalService` and `GoalService`
   - Business logic: Validate team member exists before creating goal
4. Create `GoalsController`:
   - `POST /api/goals`
   - `PUT /api/goals/{id}/toggle`
   - `DELETE /api/goals/{id}`
5. Add validation: Goal text 1-500 chars, team member ID exists
6. Test all goal endpoints with cURL

**Frontend Tasks**:
1. Update `api.ts`:
   - `createGoal(teamMemberId, goalText)`
   - `toggleGoalCompletion(goalId)`
   - `deleteGoal(goalId)`
2. Create `GoalInputForm.vue`:
   - Textarea with 500-char limit
   - Character counter
   - Validation (client-side)
   - Submit and Cancel buttons
3. Update `TeamMemberCard.vue`:
   - Add "Add Goal" button (show only if `isCurrentUser=true`)
   - Show `GoalInputForm` on button click
   - Handle `goal-created` event → Add goal to local state
4. Update `GoalItem.vue`:
   - Add checkbox → Emit `goal-toggled` event
   - Add delete button → Show confirmation dialog → Emit `goal-deleted` event
5. Implement optimistic updates:
   - Toggle: Update UI immediately, rollback on error
   - Delete: Remove from UI immediately, rollback on error
6. Test all goal operations

**Acceptance Criteria**:
- [ ] User can add goal (max 500 chars)
- [ ] Goal appears immediately in their card
- [ ] User can toggle goal completion (checkbox)
- [ ] Completed goals show strikethrough
- [ ] User can delete goal (with confirmation)
- [ ] All operations work with optimistic UI updates

---

### Phase 4: User Story 3 - Update Mood (P1)
**Time Estimate**: 3-4 hours

**Backend Tasks**:
1. Create DTO: `MoodUpdateRequest.cs`
2. Create `Mood` enum (Great, Good, Okay, Struggling, Overwhelmed)
3. Implement `TeamMemberRepository.UpdateMoodAsync(id, mood, timestamp)`
4. Add `UpdateMoodAsync()` to `TeamMemberService`
5. Add `PUT /api/team-members/{id}/mood` to `TeamMembersController`
6. Add validation: Mood must be valid enum value
7. Test mood update endpoint

**Frontend Tasks**:
1. Update `api.ts`: `updateMood(teamMemberId, mood)`
2. Create `MoodSelector.vue`:
   - Button group with 5 mood buttons
   - Highlight current mood
   - Emit `mood-changed` event
3. Update `TeamMemberCard.vue`:
   - Show `MoodSelector` if `isCurrentUser=true`
   - Show mood badge if not current user
   - Handle `mood-changed` event → Call API → Update local state
4. Add mood-based styling:
   - Card background color based on mood (light tint)
   - Mood badge colors (DaisyUI classes)
5. Implement optimistic update
6. Test mood updates

**Acceptance Criteria**:
- [ ] User can select mood from 5 options (Great, Good, Okay, Struggling, Overwhelmed)
- [ ] Current mood is visually highlighted
- [ ] Card background color changes based on mood
- [ ] Mood update is instant (optimistic UI)
- [ ] Other team members see updated mood (on refresh)

---

### Phase 5: User Story 4 - View Stats (P2 - Optional)
**Time Estimate**: 3-4 hours

**Backend Tasks**:
1. Create DTO: `StatsResponse.cs`
2. Create `IStatsService` and `StatsService`:
   - `GetDailyStatsAsync(date)` - Query goals and moods for given date
3. Create `StatsController`:
   - `GET /api/stats?date={YYYY-MM-DD}` (defaults to today)
4. Implement queries:
   - Total goals created today
   - Completed goals today
   - Completion rate
   - Mood breakdown (count per mood)
5. Test stats endpoint

**Frontend Tasks**:
1. Update `api.ts`: `getStats(date?)`
2. Create `StatsPanel.vue`:
   - Fetch stats on mount
   - Display using DaisyUI `stats` component:
     - Total goals
     - Completed goals
     - Completion rate (%)
   - Display mood breakdown (badges or bar chart)
3. Add `StatsPanel` to `DashboardView.vue`
4. Test stats display

**Acceptance Criteria**:
- [ ] Stats panel shows today's goal statistics
- [ ] Completion rate is accurate (handles 0 goals gracefully)
- [ ] Mood breakdown shows count for each mood
- [ ] Stats update when goals/moods change (on refresh)

---

### Phase 6: Polish & Documentation
**Time Estimate**: 2-3 hours

1. **Error Handling**:
   - Add `ExceptionHandlingMiddleware` for consistent error responses
   - Add try/catch blocks in frontend API calls
   - Show user-friendly error messages (toasts or alerts)

2. **Loading States**:
   - Show spinners during API calls
   - Disable buttons during submissions

3. **Validation**:
   - Ensure client-side and server-side validation match
   - Show validation errors inline (e.g., "Goal text too long")

4. **Styling**:
   - Ensure consistent spacing with DaisyUI utility classes
   - Add hover states to buttons
   - Responsive layout (desktop only, no mobile media queries needed)

5. **Documentation**:
   - Update README.md with setup instructions
   - Add code comments for complex logic
   - Document environment variables

6. **Testing** (if time permits):
   - Manual testing of all user stories
   - Browser testing (Chrome, Firefox, Safari)
   - Test edge cases (empty goals, null moods, etc.)

---

## Performance Targets (Success Criteria)

| Metric | Target | Measurement |
|--------|--------|-------------|
| Page Load Time | < 3 seconds | Chrome DevTools Network tab (20 members, 5 goals each) |
| API Latency (p95) | < 200ms | Postman or browser Network tab (under load) |
| Database Query Time | < 50ms | SQLite EXPLAIN QUERY PLAN, measure with logging |
| Bundle Size (frontend) | < 500 KB (gzipped) | `npm run build` output |
| Lighthouse Performance | > 90 | Chrome DevTools Lighthouse |

---

## Deployment Strategy (MVP)

**Target**: Single desktop machine (local deployment)

**Steps**:
1. **Backend**:
   - Publish: `dotnet publish -c Release -o ./publish`
   - Copy `publish/` folder to target machine
   - Run: `./backend` (or `backend.exe` on Windows)

2. **Frontend**:
   - Build: `npm run build` (creates `dist/` folder)
   - Serve: Use simple HTTP server (e.g., `npx serve dist`)

3. **Database**:
   - SQLite file (`team-tracker.db`) is created automatically on first backend run
   - No separate database server needed

**Future Deployment** (out of scope):
- Docker containers
- Cloud hosting (Azure, AWS, etc.)
- Reverse proxy (nginx) for frontend
- HTTPS with SSL certificates

---

## Risk Mitigation

### Risk 1: Last-Write-Wins Conflicts
**Mitigation**: Acceptable for MVP (documented in spec). Future: Add optimistic concurrency with timestamps.

### Risk 2: SQLite Write Contention
**Mitigation**: SQLite handles serialization automatically. Monitor if performance issues arise.

### Risk 3: Large Dataset Performance
**Mitigation**: Spec assumes ≤20 members with "a few" goals per day. If needed, add pagination to goal lists.

### Risk 4: Browser Compatibility
**Mitigation**: Target latest 2 versions of Chrome/Firefox/Safari. Add browser detection if issues arise.

---

## Testing Strategy

**MVP Scope**: Manual testing only (no automated tests unless explicitly requested)

**Test Plan**:
1. **Unit Tests** (if requested):
   - Backend: xUnit for services
   - Frontend: Vitest for composables

2. **Integration Tests** (if requested):
   - Backend: xUnit + in-memory SQLite for repository tests
   - Frontend: Vitest + Vue Test Utils for component tests

3. **Contract Tests** (if requested):
   - Verify API responses match frontend expectations
   - Use Postman collections or xUnit integration tests

4. **E2E Tests** (out of scope):
   - Not included in MVP

---

## Next Steps

1. ✅ Implementation plan complete
2. ➡️ Run `/speckit.tasks` to generate dependency-ordered task list
3. ➡️ Run `/speckit.implement` to begin development (or implement manually following this plan)
4. ➡️ Test each user story independently before moving to next priority

---

## References

- **Specification**: [spec.md](spec.md)
- **Data Model**: [data-model.md](data-model.md)
- **API Contracts**: [contracts/](contracts/)
- **Quickstart Guide**: [quickstart.md](quickstart.md)
- **Research**: [research.md](research.md)
- **Constitution**: [../../.specify/memory/constitution.md](../../.specify/memory/constitution.md)
