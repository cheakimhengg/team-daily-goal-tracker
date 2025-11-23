# Tasks: Team Daily Goal Tracker with Mood Sync

**Input**: Design documents from `/specs/001-team-daily-goal-tracker/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md, quickstart.md

**Tests**: Tests are NOT requested in the specification - this implementation focuses on functional MVP delivery only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US0, US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Frontend**: `frontend/src/components/`, `frontend/src/views/`, `frontend/src/services/`, `frontend/src/types/`, `frontend/src/composables/`
- **Backend**: `backend/Controllers/`, `backend/Models/`, `backend/Services/`, `backend/Data/`, `backend/Middleware/`, `backend/Exceptions/`
- All tasks must use exact file paths from plan.md structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create project root directory structure (frontend/ and backend/ directories)
- [X] T002 Initialize Vue 3 + TypeScript + Vite project in frontend/ using `npm create vite@latest frontend -- --template vue-ts`
- [X] T003 [P] Initialize .NET 8 Web API project in backend/ using `dotnet new webapi -n backend`
- [X] T004 [P] Install and configure Tailwind CSS and DaisyUI in frontend/package.json
- [X] T005 [P] Configure Tailwind config at frontend/tailwind.config.js with DaisyUI plugin
- [X] T006 [P] Install Dapper and Microsoft.Data.Sqlite packages in backend/backend.csproj
- [X] T007 Create frontend/.env file with VITE_API_BASE_URL=http://localhost:5000
- [X] T008 Configure CORS in backend/Program.cs to allow http://localhost:5173 origin
- [X] T009 Create backend/Data/Migrations/001_InitialSchema.sql with TeamMembers and Goals table DDL

**Checkpoint**: Project structure ready - both frontend and backend can start independently

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T010 Create SQLite database connection factory at backend/Data/IDbConnectionFactory.cs (interface)
- [X] T011 [P] Implement SqliteConnectionFactory at backend/Data/SqliteConnectionFactory.cs with connection string "Data Source=Data/team-tracker.db"
- [X] T012 [P] Create TeamMember entity model at backend/Models/TeamMember.cs with Id, Name, CurrentMood, MoodUpdatedAt properties
- [X] T013 [P] Create Goal entity model at backend/Models/Goal.cs with Id, TeamMemberId, GoalText, CreatedAt, IsCompleted properties
- [X] T014 [P] Create Mood enum at backend/Models/Mood.cs with values: Great, Good, Okay, Struggling, Overwhelmed
- [X] T015 [P] Create custom exceptions at backend/Exceptions/TeamMemberNotFoundException.cs and backend/Exceptions/GoalNotFoundException.cs
- [X] T016 [P] Create global exception handling middleware at backend/Middleware/ExceptionHandlingMiddleware.cs
- [X] T017 Configure dependency injection in backend/Program.cs: register IDbConnectionFactory, repositories, and services
- [X] T018 Add database migration execution in backend/Program.cs to run 001_InitialSchema.sql on startup
- [X] T019 Seed initial team members data in 001_InitialSchema.sql (insert 4 sample team members: Alice, Bob, Charlie, Diana)
- [X] T020 [P] Create TypeScript interface at frontend/src/types/TeamMember.ts with id, name, currentMood, moodUpdatedAt, goals properties
- [X] T021 [P] Create TypeScript interface at frontend/src/types/Goal.ts with id, teamMemberId, goalText, createdAt, isCompleted properties
- [X] T022 [P] Create TypeScript enum at frontend/src/types/Mood.ts with Great, Good, Okay, Struggling, Overwhelmed values
- [X] T023 [P] Create TypeScript interfaces at frontend/src/types/ApiResponses.ts for all API response wrappers (TeamMembersResponse, GoalResponse, etc.)
- [X] T024 Create API service base at frontend/src/services/api.ts with fetchJSON helper function and API_BASE_URL configuration

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 0 - Identify Self on Dashboard (Priority: P1) 🎯 MVP Foundation

**Goal**: Enable simple identity selection via dropdown so users can identify themselves before making updates

**Independent Test**: Open dashboard, select a name from dropdown, verify identity is stored in session storage and persists until page refresh

### Implementation for User Story 0

- [X] T025 [P] [US0] Create ITeamMemberRepository interface at backend/Data/Repositories/ITeamMemberRepository.cs with GetAllAsync and GetByIdAsync signatures
- [X] T026 [US0] Implement TeamMemberRepository at backend/Data/Repositories/TeamMemberRepository.cs with GetAllAsync(includeGoals: false) using Dapper query "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers ORDER BY Name"
- [X] T027 [P] [US0] Create ITeamMemberService interface at backend/Services/ITeamMemberService.cs with GetAllAsync signature
- [X] T028 [US0] Implement TeamMemberService at backend/Services/TeamMemberService.cs with GetAllAsync method delegating to repository
- [X] T029 [US0] Create TeamMembersController at backend/Controllers/TeamMembersController.cs with GET /api/team-members endpoint (includeGoals query parameter, default false)
- [X] T030 [P] [US0] Add getTeamMembers(includeGoals: boolean) function to frontend/src/services/api.ts calling GET /api/team-members
- [X] T031 [P] [US0] Create composable at frontend/src/composables/useIdentity.ts with currentUserId ref, session storage persistence, and watchers
- [X] T032 [P] [US0] Create IdentitySelector component at frontend/src/components/IdentitySelector.vue with DaisyUI select dropdown showing team member names
- [X] T033 [US0] Create DashboardView at frontend/src/views/DashboardView.vue: fetch team members on mount, integrate IdentitySelector, emit identity-selected event
- [X] T034 [US0] Update frontend/src/App.vue to set up Vue Router and render DashboardView as main route
- [X] T035 [US0] Configure Vue Router in frontend/src/main.ts with single route "/" → DashboardView

**Checkpoint**: At this point, users can select their identity from dropdown. Identity is stored in session storage and cleared on page refresh.

---

## Phase 4: User Story 1 - View Team Dashboard (Priority: P1) 🎯 MVP Core

**Goal**: Display all team members with their goals and moods in a card-based dashboard layout

**Independent Test**: Open dashboard, verify all team members appear as cards showing name, mood indicator, and goals list. Test with seed data (4 team members with various goals/moods).

### Implementation for User Story 1

- [X] T036 [US1] Update TeamMemberRepository.GetAllAsync to support includeGoals=true with Dapper multi-mapping: LEFT JOIN Goals, split on "Id", hydrate TeamMember.Goals collection
- [X] T037 [US1] Update TeamMemberService.GetAllAsync to pass includeGoals parameter to repository
- [X] T038 [P] [US1] Create TeamMemberCard component at frontend/src/components/TeamMemberCard.vue with props: teamMember, isCurrentUser; displays name, mood badge, goals list, conditionally shows edit controls
- [X] T039 [P] [US1] Create GoalItem component at frontend/src/components/GoalItem.vue with props: goal, canEdit; displays goal text with checkbox (if canEdit), strikethrough if completed, delete button
- [X] T040 [US1] Update DashboardView.vue: call getTeamMembers(includeGoals=true) on mount, render TeamMemberCard for each team member with v-for, pass isCurrentUser based on currentUserId from useIdentity
- [X] T041 [US1] Add computed property moodBackgroundClass to TeamMemberCard.vue mapping Mood enum to DaisyUI color classes (bg-success/10 for Great, bg-error/10 for Overwhelmed)
- [X] T042 [US1] Add computed property moodBadgeClass to TeamMemberCard.vue mapping Mood enum to DaisyUI badge classes (badge-success for Great, badge-error for Overwhelmed)

**Checkpoint**: At this point, dashboard displays all team members with their goals and moods. Cards show read-only view with color-coded mood indicators.

---

## Phase 5: User Story 2 - Set Personal Daily Goals (Priority: P1) 🎯 MVP Essential

**Goal**: Enable users to add, toggle completion, and delete their own goals

**Independent Test**: Select identity, add a goal (verify it appears), toggle goal completion (verify strikethrough), delete goal (verify removal). All operations should work with optimistic UI updates.

### Implementation for User Story 2

- [X] T043 [P] [US2] Create DTOs at backend/Models/DTOs/GoalCreateRequest.cs with TeamMemberId and GoalText properties, add DataAnnotations validation (Required, StringLength 1-500)
- [X] T044 [P] [US2] Create IGoalRepository interface at backend/Data/Repositories/IGoalRepository.cs with InsertAsync, GetByIdAsync, ToggleCompletionAsync, DeleteAsync signatures
- [X] T045 [US2] Implement GoalRepository at backend/Data/Repositories/GoalRepository.cs:
  - InsertAsync: "INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted) VALUES (...); SELECT last_insert_rowid()"
  - GetByIdAsync: "SELECT * FROM Goals WHERE Id = @Id"
  - ToggleCompletionAsync: "UPDATE Goals SET IsCompleted = CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END WHERE Id = @Id"
  - DeleteAsync: "DELETE FROM Goals WHERE Id = @Id"
- [X] T046 [P] [US2] Create IGoalService interface at backend/Services/IGoalService.cs with CreateAsync, ToggleCompletionAsync, DeleteAsync signatures
- [X] T047 [US2] Implement GoalService at backend/Services/GoalService.cs:
  - CreateAsync: validate TeamMemberId exists via TeamMemberRepository, create Goal entity, call repository.InsertAsync
  - ToggleCompletionAsync: call repository.ToggleCompletionAsync, fetch updated goal, throw GoalNotFoundException if not found
  - DeleteAsync: call repository.DeleteAsync, throw GoalNotFoundException if rows affected = 0
- [X] T048 [US2] Create GoalsController at backend/Controllers/GoalsController.cs with three endpoints:
  - POST /api/goals (accepts GoalCreateRequest, returns 201 Created with goal data)
  - PUT /api/goals/{id}/toggle (empty body, returns 200 OK with updated goal)
  - DELETE /api/goals/{id} (returns 204 No Content on success)
- [X] T049 [P] [US2] Add API functions to frontend/src/services/api.ts:
  - createGoal(teamMemberId, goalText) → POST /api/goals
  - toggleGoalCompletion(goalId) → PUT /api/goals/{id}/toggle
  - deleteGoal(goalId) → DELETE /api/goals/{id}
- [X] T050 [P] [US2] Create GoalInputForm component at frontend/src/components/GoalInputForm.vue: textarea with 500 char limit, character counter, validation, submit/cancel buttons, emits goal-created event
- [X] T051 [US2] Update TeamMemberCard.vue: add "Add Goal" button (show only if isCurrentUser), toggle showGoalForm state, integrate GoalInputForm, handle goal-created event → call createGoal API → add to local teamMember.goals array
- [X] T052 [US2] Update GoalItem.vue: add checkbox input → emit goal-toggled event, add delete button → show confirmation dialog → emit goal-deleted event
- [X] T053 [US2] Update TeamMemberCard.vue: handle goal-toggled event → call toggleGoalCompletion API with optimistic update (update local state immediately, rollback on error)
- [X] T054 [US2] Update TeamMemberCard.vue: handle goal-deleted event → call deleteGoal API with optimistic update (remove from local goals array immediately, rollback on error)

**Checkpoint**: At this point, users can fully manage their goals (add, complete, delete). All goal operations work with optimistic UI updates and proper error handling.

---

## Phase 6: User Story 3 - Update Personal Mood Status (Priority: P1) 🎯 MVP Differentiator

**Goal**: Enable users to select and update their mood, with visual indicators (colors) displayed on their card

**Independent Test**: Select identity, click mood button (verify mood badge updates and card background color changes), select different mood (verify update). Other users see updated mood on page refresh.

### Implementation for User Story 3

- [X] T055 [P] [US3] Create DTO at backend/Models/DTOs/MoodUpdateRequest.cs with Mood property, add DataAnnotations validation (Required, EnumDataType)
- [X] T056 [US3] Add UpdateMoodAsync(id, mood, timestamp) to ITeamMemberRepository interface at backend/Data/Repositories/ITeamMemberRepository.cs
- [X] T057 [US3] Implement UpdateMoodAsync in TeamMemberRepository.cs: "UPDATE TeamMembers SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp WHERE Id = @Id" using Dapper
- [X] T058 [US3] Add UpdateMoodAsync(id, mood) to ITeamMemberService interface at backend/Services/ITeamMemberService.cs
- [X] T059 [US3] Implement UpdateMoodAsync in TeamMemberService.cs: fetch team member by id, throw TeamMemberNotFoundException if null, update mood and timestamp, call repository.UpdateMoodAsync, return updated team member
- [X] T060 [US3] Add PUT /api/team-members/{id}/mood endpoint to TeamMembersController.cs: accept MoodUpdateRequest, validate ModelState, call service.UpdateMoodAsync, return 200 OK with updated team member
- [X] T061 [P] [US3] Add updateMood(teamMemberId, mood) function to frontend/src/services/api.ts calling PUT /api/team-members/{id}/mood
- [X] T062 [P] [US3] Create MoodSelector component at frontend/src/components/MoodSelector.vue: DaisyUI button group with 5 mood buttons (Great, Good, Okay, Struggling, Overwhelmed), highlight currentMood, emit mood-changed event
- [X] T063 [US3] Add getMoodEmoji helper function to MoodSelector.vue mapping Mood enum to emojis (😊, 🙂, 😐, 😟, 😰)
- [X] T064 [US3] Update TeamMemberCard.vue: integrate MoodSelector (show if isCurrentUser), show mood badge if not current user, handle mood-changed event → call updateMood API with optimistic update
- [X] T065 [US3] Add mood-based styling to TeamMemberCard.vue: card background color changes based on teamMember.currentMood using computed property (light tint for each mood)
- [X] T066 [US3] Test mood updates: verify card background color changes, mood badge updates, optimistic UI updates work, rollback on error

**Checkpoint**: At this point, all P1 user stories (US0, US1, US2, US3) are complete. Users can identify themselves, view dashboard, manage goals, and update moods. This is the MVP!

---

## Phase 7: User Story 4 - View Team Statistics Summary (Priority: P2)

**Goal**: Display aggregated statistics (total goals, completed goals, completion rate, mood breakdown) in a stats panel

**Independent Test**: View stats panel, verify accurate counts (total goals, completed goals today, mood distribution). Add/remove goals or change moods, refresh page, verify stats update.

### Implementation for User Story 4

- [ ] T067 [P] [US4] Create DTO at backend/Models/DTOs/StatsResponse.cs with Date, TotalGoals, CompletedGoals, CompletionRate, MoodBreakdown (Dictionary<string, int>), TeamSize properties
- [ ] T068 [P] [US4] Create IStatsService interface at backend/Services/IStatsService.cs with GetDailyStatsAsync(date) signature
- [ ] T069 [US4] Implement StatsService at backend/Services/StatsService.cs with GetDailyStatsAsync method:
  - Query 1: "SELECT COUNT(*) FROM Goals WHERE DATE(CreatedAt) = @Date" (total goals)
  - Query 2: "SELECT COUNT(*) FROM Goals WHERE DATE(CreatedAt) = @Date AND IsCompleted = 1" (completed goals)
  - Query 3: "SELECT CurrentMood, COUNT(*) FROM TeamMembers WHERE CurrentMood IS NOT NULL GROUP BY CurrentMood" (mood breakdown)
  - Query 4: "SELECT COUNT(*) FROM TeamMembers" (team size)
  - Calculate completion rate: (completedGoals / totalGoals) * 100, handle division by zero
  - Return StatsResponse DTO
- [ ] T070 [US4] Create StatsController at backend/Controllers/StatsController.cs with GET /api/stats endpoint: accept optional date query parameter (defaults to today), call service.GetDailyStatsAsync, return 200 OK with stats data
- [ ] T071 [P] [US4] Add getStats(date?: string) function to frontend/src/services/api.ts calling GET /api/stats with optional date parameter
- [ ] T072 [P] [US4] Create TypeScript interface at frontend/src/types/Stats.ts with date, totalGoals, completedGoals, completionRate, moodBreakdown, teamSize properties
- [ ] T073 [P] [US4] Create StatsPanel component at frontend/src/components/StatsPanel.vue: fetch stats on mount, display using DaisyUI stats component (total goals, completed goals, completion rate), display mood breakdown with badges
- [ ] T074 [US4] Update DashboardView.vue: integrate StatsPanel component at top or side of dashboard
- [ ] T075 [US4] Add visual indicators to StatsPanel.vue: completion rate color (red 0-30%, yellow 31-70%, green 71-100%), mood colors match MoodSelector colors

**Checkpoint**: All user stories complete (US0-US4). Stats panel provides team-level insights. Application is feature-complete per specification.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories, error handling, and deployment readiness

- [ ] T076 [P] Add error toast notifications in DashboardView.vue for API failures (use DaisyUI alert component)
- [ ] T077 [P] Add loading spinners during API calls in DashboardView.vue, TeamMemberCard.vue, GoalInputForm.vue (use DaisyUI loading component)
- [ ] T078 Add validation error display in GoalInputForm.vue: show inline error messages for empty text, text too long (> 500 chars)
- [ ] T079 Add confirmation dialogs for destructive actions: delete goal confirmation using browser confirm() or DaisyUI modal
- [ ] T080 [P] Add consistent spacing and styling with DaisyUI utility classes across all components (cards, buttons, inputs, badges)
- [ ] T081 [P] Add hover states to buttons using DaisyUI btn-hover classes
- [ ] T082 Test application in Chrome, Firefox, Safari (latest 2 versions): verify layout, functionality, API calls
- [ ] T083 Test edge cases: empty goals list, null moods, extremely long goal text (truncate at 200 chars with ellipsis in display)
- [ ] T084 [P] Add README.md at project root with setup instructions (copy from quickstart.md key sections)
- [ ] T085 Verify performance targets: page load < 3 seconds (Chrome DevTools), API latency < 200ms (Network tab), Lighthouse score > 90

**Checkpoint**: Application is polished, tested, and ready for deployment. All edge cases handled, error states display user-friendly messages.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User Story 0 (Phase 3): No dependencies on other stories (foundation for US2, US3)
  - User Story 1 (Phase 4): Depends on US0 (needs GetAllAsync from US0)
  - User Story 2 (Phase 5): Depends on US0 (needs identity) and US1 (displays goals)
  - User Story 3 (Phase 6): Depends on US0 (needs identity) and US1 (displays mood)
  - User Story 4 (Phase 7): Depends on US2 and US3 (aggregates goals and moods)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

```
Phase 2 (Foundational)
    ↓
Phase 3 (US0: Identify Self) ← MVP Foundation
    ↓
Phase 4 (US1: View Dashboard) ← MVP Core
    ↓
    ├─→ Phase 5 (US2: Set Goals) ← MVP Essential
    │       ↓
    └─→ Phase 6 (US3: Update Mood) ← MVP Differentiator
            ↓
        Phase 7 (US4: View Stats) ← P2 Optional
            ↓
        Phase 8 (Polish)
```

### Within Each User Story

- Models before services
- Services before controllers/repositories
- Backend endpoints before frontend API calls
- API service before components
- Child components before parent components
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- **Setup Phase**: T002, T003, T004, T005, T006 can run in parallel (different projects)
- **Foundational Phase**: T010-T016, T020-T024 can run in parallel (different files, no dependencies)
- **US0 Implementation**: T025, T027, T030, T031, T032 can run in parallel after T026, T028, T029 complete
- **US1 Implementation**: T038, T039 can run in parallel (independent components)
- **US2 Implementation**: T043, T044, T046, T049, T050 can run in parallel (different files)
- **US3 Implementation**: T055, T061, T062 can run in parallel (different files)
- **US4 Implementation**: T067, T068, T071, T072, T073 can run in parallel after backend services complete
- **Polish Phase**: T076, T077, T080, T081, T084 can run in parallel (independent improvements)

---

## Parallel Example: User Story 2 (Set Goals)

```bash
# Launch all DTO/interface tasks together:
Task T043: Create GoalCreateRequest DTO
Task T044: Create IGoalRepository interface
Task T046: Create IGoalService interface
Task T049: Add API functions to api.ts
Task T050: Create GoalInputForm component

# Then launch implementation tasks sequentially:
Task T045: Implement GoalRepository (depends on T044)
Task T047: Implement GoalService (depends on T044, T046)
Task T048: Create GoalsController (depends on T047)

# Finally integrate frontend:
Task T051: Update TeamMemberCard with Add Goal button (depends on T050)
Task T052: Update GoalItem with toggle/delete (depends on T049)
Task T053: Handle goal-toggled event (depends on T049, T052)
Task T054: Handle goal-deleted event (depends on T049, T052)
```

---

## Implementation Strategy

### MVP First (User Stories 0-3 Only)

1. Complete Phase 1: Setup (T001-T009)
2. Complete Phase 2: Foundational (T010-T024) - CRITICAL, blocks all stories
3. Complete Phase 3: US0 - Identify Self (T025-T035) - MVP Foundation
4. Complete Phase 4: US1 - View Dashboard (T036-T042) - MVP Core
5. Complete Phase 5: US2 - Set Goals (T043-T054) - MVP Essential
6. Complete Phase 6: US3 - Update Mood (T055-T066) - MVP Differentiator
7. **STOP and VALIDATE**: Test all P1 features independently
8. Deploy/demo MVP (US0-US3)

**MVP Scope**: 66 tasks (T001-T066)
**Timeline**: 3-4 days

### Incremental Delivery (Add US4 Stats)

1. Complete MVP (US0-US3) → Test independently → Deploy/Demo
2. Add Phase 7: US4 - View Stats (T067-T075) → Test independently → Deploy/Demo
3. Each story adds value without breaking previous stories

**Full Feature Scope**: 75 tasks (T001-T075)
**Timeline**: 4-5 days

### Final Polish

1. Complete all user stories (US0-US4)
2. Add Phase 8: Polish & Cross-Cutting (T076-T085)
3. Final testing and deployment

**Complete Implementation**: 85 tasks (T001-T085)
**Timeline**: 5-6 days

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group of tasks
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Tests are NOT included per specification (manual testing only)

---

## Task Summary

**Total Tasks**: 85
- Phase 1 (Setup): 9 tasks
- Phase 2 (Foundational): 15 tasks
- Phase 3 (US0): 11 tasks
- Phase 4 (US1): 7 tasks
- Phase 5 (US2): 12 tasks
- Phase 6 (US3): 12 tasks
- Phase 7 (US4 - P2): 9 tasks
- Phase 8 (Polish): 10 tasks

**Parallel Opportunities**: 35+ tasks can run in parallel (marked with [P])

**MVP Scope**: T001-T066 (66 tasks, ~3-4 days)

**Independent Test Criteria**:
- **US0**: Select identity from dropdown, verify session storage persistence
- **US1**: View dashboard with all team members, goals, and moods
- **US2**: Add/toggle/delete goals with optimistic updates
- **US3**: Update mood with visual indicators and color changes
- **US4**: View stats panel with accurate aggregated data

**Suggested MVP**: Complete US0, US1, US2, US3 only (skip US4 for initial release)
