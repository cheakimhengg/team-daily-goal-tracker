# Research: Team Daily Goal Tracker with Mood Sync

**Feature**: [spec.md](spec.md)
**Created**: 2025-11-20
**Purpose**: Technology decisions and implementation patterns for the MVP

---

## Technology Stack Rationale

### Frontend: Vue 3 + TypeScript + DaisyUI

**Vue 3 Composition API**
- **Why**: Mandated by constitution. Composition API provides better TypeScript integration and code organization for dashboard-style applications
- **Pattern**: Use `<script setup>` syntax throughout for consistency and reduced boilerplate
- **State Management**: Plain `ref()` and `reactive()` from Composition API for simple state. No need for Pinia given limited state scope (current user identity, dashboard data)
- **Reactivity**: Leverage Vue's reactivity system for automatic UI updates when goals/moods change

**TypeScript (Strict Mode)**
- **Why**: Mandated by constitution. Catches errors at compile-time, especially critical for API response types and data models
- **Pattern**: Define interfaces for all API responses (`TeamMemberResponse`, `GoalResponse`, etc.)
- **Benefits**: IntelliSense support, refactoring safety, self-documenting code

**DaisyUI Components**
- **Why**: Mandated by constitution. Provides pre-styled Tailwind CSS components that match our desktop-only requirement
- **Components to use**:
  - `select` for identity dropdown (US0)
  - `card` for team member cards (US1)
  - `btn` for mood selection and goal actions (US2, US3)
  - `input` + `textarea` for goal input form (US2)
  - `badge` for mood indicators (US3)
  - `stats` for statistics panel (US4)
- **Theme**: Use default DaisyUI theme (light mode only per MVP scope)

**Vite Build Tool**
- **Why**: Fast HMR (Hot Module Replacement) for development, optimized production builds
- **Configuration**: Standard Vite + Vue 3 + TypeScript setup

### Backend: .NET 8 + Dapper + SQLite

**.NET 8 Web API**
- **Why**: Mandated by constitution. Modern, performant, excellent async support
- **Pattern**: Minimal API or Controller-based (recommend Controllers for clarity with 8 endpoints)
- **Features to use**:
  - Built-in dependency injection for services
  - Model validation attributes for request DTOs
  - Exception middleware for consistent error responses

**Dapper ORM**
- **Why**: Mandated by constitution. Lightweight, fast, gives full SQL control
- **Pattern**:
  - Repository pattern for data access (`TeamMemberRepository`, `GoalRepository`)
  - Parameterized queries to prevent SQL injection
  - Manual mapping between database rows and C# models
- **Benefits**: Performance (no lazy loading overhead), explicit SQL (easier to optimize), minimal abstractions

**SQLite Database**
- **Why**: Mandated by constitution. Zero configuration, file-based, perfect for MVP local deployment
- **File location**: `backend/Data/team-tracker.db`
- **Schema management**: SQL migration scripts in `backend/Data/Migrations/`
- **Considerations**:
  - Single writer, multiple readers (fine for team size ≤20)
  - Use `TEXT` for timestamps (ISO 8601 format)
  - Use `INTEGER` for booleans (0/1)
  - Add indexes on foreign keys

---

## Architecture Decisions

### Frontend Architecture

**Component Structure**
```
frontend/src/
├── components/
│   ├── IdentitySelector.vue     (US0 - dropdown)
│   ├── TeamMemberCard.vue       (US1 - individual card)
│   ├── GoalItem.vue             (US1 - goal list item)
│   ├── GoalInputForm.vue        (US2 - add goal form)
│   ├── MoodSelector.vue         (US3 - mood buttons)
│   └── StatsPanel.vue           (US4 - statistics)
├── views/
│   └── DashboardView.vue        (Main page - composes all components)
├── services/
│   └── api.ts                   (Axios/fetch wrapper for API calls)
├── types/
│   ├── TeamMember.ts            (TypeScript interfaces)
│   ├── Goal.ts
│   └── ApiResponses.ts
├── composables/
│   └── useIdentity.ts           (Session identity management)
└── App.vue                      (Root component)
```

**State Management Strategy**
- **Identity**: Stored in session via `useIdentity()` composable (returns `ref<string | null>`)
- **Dashboard Data**: Fetched on mount, stored in `DashboardView.vue` as `ref<TeamMember[]>`
- **No global store**: Data is simple enough to pass via props and emit events
- **Optimistic updates**: Update UI immediately, rollback on API failure

**API Communication**
- Use `fetch()` API (native, no dependencies) wrapped in `services/api.ts`
- Error handling: Try/catch with user-friendly error messages
- Loading states: Show spinners during API calls (meets < 2s page load requirement)

### Backend Architecture

**Project Structure**
```
backend/
├── Controllers/
│   ├── TeamMembersController.cs  (GET /api/team-members)
│   ├── GoalsController.cs        (GET, POST, PUT, DELETE /api/goals)
│   └── StatsController.cs        (GET /api/stats)
├── Models/
│   ├── TeamMember.cs             (Entity models)
│   ├── Goal.cs
│   └── DTOs/                     (Request/Response DTOs)
│       ├── GoalCreateRequest.cs
│       ├── GoalUpdateRequest.cs
│       ├── MoodUpdateRequest.cs
│       └── StatsResponse.cs
├── Services/
│   ├── ITeamMemberService.cs     (Business logic interfaces)
│   ├── TeamMemberService.cs
│   ├── IGoalService.cs
│   └── GoalService.cs
├── Data/
│   ├── IDbConnectionFactory.cs   (SQLite connection management)
│   ├── SqliteConnectionFactory.cs
│   ├── Repositories/
│   │   ├── ITeamMemberRepository.cs
│   │   ├── TeamMemberRepository.cs
│   │   ├── IGoalRepository.cs
│   │   └── GoalRepository.cs
│   ├── Migrations/
│   │   └── 001_InitialSchema.sql
│   └── team-tracker.db           (SQLite database file)
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
└── Program.cs                    (Entry point, DI configuration)
```

**Layered Architecture**
1. **Controllers**: HTTP request/response handling, validation, status codes
2. **Services**: Business logic, orchestration between repositories
3. **Repositories**: Data access, Dapper queries, transaction management
4. **Models**: Entity classes and DTOs

**Dependency Injection**
```csharp
// Program.cs
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();
builder.Services.AddScoped<IGoalService, GoalService>();
```

**Error Handling Strategy**
- Custom exception middleware returns consistent JSON error responses
- Validation errors: 400 Bad Request with field-level errors
- Not found: 404 with descriptive message
- Server errors: 500 with generic message (log details internally)

---

## Data Flow

### User Story 0: Identify Self (US0)

**Frontend Flow**:
1. `DashboardView.vue` mounts → calls `GET /api/team-members`
2. `IdentitySelector.vue` receives team members as prop → renders dropdown
3. User selects name → emits `@identity-selected` event with `teamMemberId`
4. `useIdentity()` composable stores ID in session storage (key: `currentUserId`)
5. Parent component re-renders affected children with identity context

**Backend Flow**:
1. `GET /api/team-members` → `TeamMembersController.GetAll()`
2. Controller calls `TeamMemberService.GetAllAsync()`
3. Service calls `TeamMemberRepository.GetAllAsync()`
4. Repository executes Dapper query: `SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers`
5. Returns `List<TeamMember>` → serialize to JSON

### User Story 1: View Dashboard (US1)

**Frontend Flow**:
1. `DashboardView.vue` mounts (after identity selected)
2. Calls `GET /api/team-members?includeGoals=true`
3. Receives array of team members with nested goals
4. Passes each `TeamMember` to `TeamMemberCard.vue` as prop
5. `TeamMemberCard.vue` renders goals via `GoalItem.vue` v-for loop

**Backend Flow**:
1. `GET /api/team-members?includeGoals=true` → `TeamMembersController.GetAll(includeGoals)`
2. Service calls repository with join query:
   ```sql
   SELECT tm.Id, tm.Name, tm.CurrentMood, tm.MoodUpdatedAt,
          g.Id, g.GoalText, g.CreatedAt, g.IsCompleted
   FROM TeamMembers tm
   LEFT JOIN Goals g ON tm.Id = g.TeamMemberId
   ORDER BY tm.Name, g.CreatedAt DESC
   ```
3. Repository uses Dapper multi-mapping to hydrate `TeamMember.Goals` collection
4. Returns nested object graph

### User Story 2: Set Goals (US2)

**Frontend Flow**:
1. User clicks "Add Goal" on their card → `GoalInputForm.vue` shows modal/inline form
2. User types goal text (validates ≤500 chars client-side)
3. User submits → calls `POST /api/goals` with `{ teamMemberId, goalText }`
4. On success: Add new goal to local state, close form
5. On error: Show validation message

**Backend Flow**:
1. `POST /api/goals` → `GoalsController.Create(GoalCreateRequest dto)`
2. Controller validates: `dto.GoalText` length ≤500, `dto.TeamMemberId` exists
3. Service calls repository: `GoalRepository.InsertAsync(goal)`
4. Repository executes:
   ```sql
   INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
   VALUES (@TeamMemberId, @GoalText, @CreatedAt, 0)
   ```
5. Returns `201 Created` with new goal object (includes generated `Id`)

### User Story 3: Update Mood (US3)

**Frontend Flow**:
1. User clicks mood button on their card → `MoodSelector.vue` emits `@mood-changed` with mood enum
2. Parent calls `PUT /api/team-members/{id}/mood` with `{ mood }`
3. On success: Update local `TeamMember.currentMood` and `moodUpdatedAt`
4. Card background color updates reactively via computed property

**Backend Flow**:
1. `PUT /api/team-members/{id}/mood` → `TeamMembersController.UpdateMood(id, MoodUpdateRequest dto)`
2. Controller validates: `id` exists, `dto.Mood` is valid enum value
3. Service calls repository: `TeamMemberRepository.UpdateMoodAsync(id, mood, DateTime.UtcNow)`
4. Repository executes:
   ```sql
   UPDATE TeamMembers
   SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp
   WHERE Id = @Id
   ```
5. Returns `200 OK` with updated team member object

### User Story 4: View Stats (US4) - Priority P2

**Frontend Flow**:
1. `StatsPanel.vue` mounts → calls `GET /api/stats`
2. Receives aggregated statistics
3. Renders using DaisyUI `stats` component with metrics:
   - Total goals today
   - Completed goals today
   - Team mood breakdown (count per mood)
   - Completion rate percentage

**Backend Flow**:
1. `GET /api/stats` → `StatsController.GetDailyStats()`
2. Service calls multiple repository methods:
   ```sql
   -- Total goals today
   SELECT COUNT(*) FROM Goals WHERE DATE(CreatedAt) = DATE('now')

   -- Completed goals today
   SELECT COUNT(*) FROM Goals WHERE DATE(CreatedAt) = DATE('now') AND IsCompleted = 1

   -- Mood breakdown
   SELECT CurrentMood, COUNT(*) as Count FROM TeamMembers GROUP BY CurrentMood
   ```
3. Combines results into `StatsResponse` DTO
4. Returns `200 OK` with statistics object

---

## Performance Considerations

### Frontend Performance
- **Bundle size**: Use Vite tree-shaking to minimize JavaScript bundle
- **Lazy loading**: Not needed for MVP (single dashboard page)
- **API calls**: Batch initial data load (team members + goals in one request)
- **Re-renders**: Use Vue's `v-memo` or `key` attribute to prevent unnecessary re-renders of team cards

### Backend Performance
- **Database indexing**: Add index on `Goals.TeamMemberId` for fast filtering
- **Connection pooling**: Use `Microsoft.Data.Sqlite` connection pooling (default)
- **Response caching**: Not needed for MVP (data changes frequently)
- **Query optimization**: Use Dapper's multi-mapping to avoid N+1 queries

### Target Metrics (from Success Criteria)
- **Page load**: < 3 seconds for 20 team members with 5 goals each
- **API latency**: < 200ms p95 (Dapper + SQLite easily achieves this)
- **Lighthouse score**: > 90 (DaisyUI + minimal JavaScript helps)

---

## Testing Strategy

### Frontend Testing (if requested)
- **Unit tests**: Vitest for composables (`useIdentity.ts`)
- **Component tests**: Vitest + Vue Test Utils for isolated component testing
- **E2E tests**: Not in MVP scope

### Backend Testing (if requested)
- **Unit tests**: xUnit for services (mock repositories)
- **Integration tests**: xUnit + in-memory SQLite for repository tests
- **Contract tests**: Verify API responses match frontend expectations

**Test Priority**: Only add tests if explicitly requested in specification (not in current MVP scope)

---

## Security Considerations

### MVP Security (minimal authentication)
- **No passwords**: Per spec, no authentication system
- **SQL injection**: Use Dapper parameterized queries (prevents injection)
- **XSS prevention**: Vue automatically escapes HTML in templates
- **CORS**: Configure backend to allow frontend origin only (`http://localhost:5173`)
- **Input validation**: Both client-side (TypeScript) and server-side (Data Annotations)

### Future Security (out of scope)
- Add authentication (JWT tokens, OAuth)
- Implement authorization (role-based access)
- Add audit logging for data changes
- HTTPS in production

---

## Deployment Strategy

### Local Development
1. Backend: `dotnet run` in `backend/` directory (runs on `http://localhost:5000`)
2. Frontend: `npm run dev` in `frontend/` directory (runs on `http://localhost:5173`)
3. Database: SQLite file created automatically on first run

### MVP Deployment (simple local deployment)
- **Backend**: Publish as self-contained .NET app
- **Frontend**: Build with `npm run build`, serve static files
- **Database**: Copy `team-tracker.db` file with published app
- **Target**: Single desktop machine (per constitution "simple local deployment")

---

## Implementation Risks & Mitigations

### Risk 1: Last-Write-Wins Conflicts
**Scenario**: Two users update same team member's mood simultaneously
**Mitigation**: Acceptable for MVP (documented in spec edge cases). Future: Add optimistic concurrency with timestamps.

### Risk 2: SQLite Write Contention
**Scenario**: Multiple users adding goals at exact same moment
**Mitigation**: SQLite handles serialization automatically. For 20 users, contention is unlikely. Monitor if needed.

### Risk 3: Large Dataset Performance
**Scenario**: Page load slows with 20 users × 50 goals each
**Mitigation**: Add pagination to goal lists if needed (currently showing all goals per spec). Spec assumes "a few" goals per person per day.

### Risk 4: Browser Compatibility
**Scenario**: DaisyUI/Tailwind not working in older browsers
**Mitigation**: Spec targets "latest 2 versions" of Chrome/Firefox/Safari. Add browser detection if issues arise.

---

## Open Questions (None - Spec Complete)

All clarifications resolved via `/speckit.clarify`. No remaining ambiguities.

---

## References

- **Vue 3 Composition API**: https://vuejs.org/guide/extras/composition-api-faq.html
- **DaisyUI Components**: https://daisyui.com/components/
- **Dapper Documentation**: https://github.com/DapperLib/Dapper
- **.NET 8 Web API**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **SQLite**: https://www.sqlite.org/docs.html

---

## Next Steps

1. ✅ Research complete
2. ➡️ Create `data-model.md` with detailed database schema
3. ➡️ Generate API contracts in `contracts/` directory
4. ➡️ Create `quickstart.md` for developer onboarding
5. ➡️ Update `plan.md` with complete implementation plan
