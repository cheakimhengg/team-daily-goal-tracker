# Data Model: Team Daily Goal Tracker with Mood Sync

**Feature**: [spec.md](spec.md)
**Created**: 2025-11-20
**Database**: SQLite 3
**ORM**: Dapper (manual mapping)

---

## Overview

The data model consists of two primary entities: **TeamMembers** and **Goals**. The design prioritizes simplicity, referential integrity, and query performance for small to medium team sizes (≤20 members).

**Key Design Decisions**:
- **Normalization**: 3NF (Third Normal Form) to eliminate redundancy
- **Timestamps**: ISO 8601 TEXT format (`YYYY-MM-DD HH:MM:SS`) for SQLite compatibility
- **Booleans**: INTEGER (0 = false, 1 = true) per SQLite convention
- **Mood Enum**: TEXT CHECK constraint (enforced at database level)
- **Foreign Keys**: Enabled with `PRAGMA foreign_keys = ON` for referential integrity
- **Indexes**: Added on foreign key columns for join performance

---

## Entity: TeamMembers

### Purpose
Represents an individual team member with their current mood state.

### Schema

```sql
CREATE TABLE TeamMembers (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL CHECK(length(Name) > 0 AND length(Name) <= 100),
    CurrentMood     TEXT CHECK(CurrentMood IN ('Great', 'Good', 'Okay', 'Struggling', 'Overwhelmed')),
    MoodUpdatedAt   TEXT
);
```

### Column Definitions

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INTEGER | PRIMARY KEY AUTOINCREMENT | Unique identifier for each team member |
| `Name` | TEXT | NOT NULL, CHECK(length ≤ 100) | Team member's display name (e.g., "Alice Smith") |
| `CurrentMood` | TEXT | CHECK(valid enum), NULL allowed | Current mood state. NULL = no mood set yet |
| `MoodUpdatedAt` | TEXT | NULL allowed | ISO 8601 timestamp of last mood update (e.g., "2025-11-20 14:30:00") |

### Business Rules
- **Name uniqueness**: Not enforced at database level (allows duplicate names for simplicity). Application handles disambiguation if needed.
- **Mood initialization**: New team members have `CurrentMood = NULL` and `MoodUpdatedAt = NULL` until first mood update.
- **Mood enumeration**: Must be one of: `'Great'`, `'Good'`, `'Okay'`, `'Struggling'`, `'Overwhelmed'`.
- **Timestamp format**: Always store UTC time in ISO 8601 format. Frontend converts to local time for display.

### Indexes

```sql
-- No additional indexes needed for TeamMembers table (primary key index is sufficient for small dataset)
```

### Sample Data

```sql
INSERT INTO TeamMembers (Name, CurrentMood, MoodUpdatedAt) VALUES
('Alice Johnson', 'Great', '2025-11-20 09:15:00'),
('Bob Smith', 'Good', '2025-11-20 09:20:00'),
('Charlie Davis', 'Okay', '2025-11-20 09:18:00'),
('Diana Chen', NULL, NULL);
```

---

## Entity: Goals

### Purpose
Represents daily goals created by team members. One team member can have multiple goals.

### Schema

```sql
CREATE TABLE Goals (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TeamMemberId    INTEGER NOT NULL,
    GoalText        TEXT NOT NULL CHECK(length(GoalText) > 0 AND length(GoalText) <= 500),
    CreatedAt       TEXT NOT NULL,
    IsCompleted     INTEGER NOT NULL DEFAULT 0 CHECK(IsCompleted IN (0, 1)),

    FOREIGN KEY (TeamMemberId) REFERENCES TeamMembers(Id) ON DELETE CASCADE
);

CREATE INDEX idx_goals_team_member ON Goals(TeamMemberId);
CREATE INDEX idx_goals_created_at ON Goals(CreatedAt);
```

### Column Definitions

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INTEGER | PRIMARY KEY AUTOINCREMENT | Unique identifier for each goal |
| `TeamMemberId` | INTEGER | NOT NULL, FOREIGN KEY | References `TeamMembers.Id`. Identifies goal owner |
| `GoalText` | TEXT | NOT NULL, CHECK(length ≤ 500) | The goal description (e.g., "Finish PR review for auth module") |
| `CreatedAt` | TEXT | NOT NULL | ISO 8601 timestamp when goal was created (e.g., "2025-11-20 10:00:00") |
| `IsCompleted` | INTEGER | NOT NULL, DEFAULT 0, CHECK(0 or 1) | Completion status: 0 = pending, 1 = completed |

### Business Rules
- **Deletion cascade**: When a team member is deleted, all their goals are automatically deleted (`ON DELETE CASCADE`).
- **Character limit**: Goal text enforced at 500 characters maximum (both database CHECK and application validation).
- **Creation timestamp**: Set by application to UTC time at moment of goal creation. Immutable after creation.
- **Completion toggle**: Goals can be marked complete/incomplete multiple times (toggle behavior).

### Indexes

```sql
-- idx_goals_team_member: Speeds up queries filtering by TeamMemberId (e.g., "get all goals for Alice")
CREATE INDEX idx_goals_team_member ON Goals(TeamMemberId);

-- idx_goals_created_at: Speeds up date-based queries for statistics (e.g., "goals created today")
CREATE INDEX idx_goals_created_at ON Goals(CreatedAt);
```

### Sample Data

```sql
INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted) VALUES
(1, 'Review PR #245 for authentication module', '2025-11-20 10:00:00', 0),
(1, 'Update API documentation for new endpoints', '2025-11-20 10:05:00', 1),
(2, 'Fix bug in dashboard loading spinner', '2025-11-20 09:30:00', 0),
(2, 'Refactor GoalService to use async/await', '2025-11-20 09:35:00', 0),
(3, 'Write unit tests for MoodSelector component', '2025-11-20 09:45:00', 1);
```

---

## Relationships

### TeamMembers ↔ Goals (One-to-Many)

```
TeamMembers (1) ----< (Many) Goals
     Id                    TeamMemberId
```

- **Cardinality**: One team member can have zero or many goals. Each goal belongs to exactly one team member.
- **Referential Integrity**: Foreign key constraint ensures `Goals.TeamMemberId` always references a valid `TeamMembers.Id`.
- **Cascade Delete**: Deleting a team member automatically deletes all their goals.

### Diagram

```
┌─────────────────────┐
│   TeamMembers       │
├─────────────────────┤
│ Id (PK)             │───┐
│ Name                │   │
│ CurrentMood         │   │
│ MoodUpdatedAt       │   │ 1
└─────────────────────┘   │
                          │
                          │
                          │ *
                      ┌───▼──────────────┐
                      │   Goals          │
                      ├──────────────────┤
                      │ Id (PK)          │
                      │ TeamMemberId (FK)│
                      │ GoalText         │
                      │ CreatedAt        │
                      │ IsCompleted      │
                      └──────────────────┘
```

---

## Database Initialization

### Migration Script: `001_InitialSchema.sql`

Located at: `backend/Data/Migrations/001_InitialSchema.sql`

```sql
-- Enable foreign key constraints (required for SQLite)
PRAGMA foreign_keys = ON;

-- Create TeamMembers table
CREATE TABLE IF NOT EXISTS TeamMembers (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL CHECK(length(Name) > 0 AND length(Name) <= 100),
    CurrentMood     TEXT CHECK(CurrentMood IN ('Great', 'Good', 'Okay', 'Struggling', 'Overwhelmed')),
    MoodUpdatedAt   TEXT
);

-- Create Goals table
CREATE TABLE IF NOT EXISTS Goals (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TeamMemberId    INTEGER NOT NULL,
    GoalText        TEXT NOT NULL CHECK(length(GoalText) > 0 AND length(GoalText) <= 500),
    CreatedAt       TEXT NOT NULL,
    IsCompleted     INTEGER NOT NULL DEFAULT 0 CHECK(IsCompleted IN (0, 1)),

    FOREIGN KEY (TeamMemberId) REFERENCES TeamMembers(Id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_goals_team_member ON Goals(TeamMemberId);
CREATE INDEX IF NOT EXISTS idx_goals_created_at ON Goals(CreatedAt);

-- Seed initial team members (optional - for development/testing)
INSERT OR IGNORE INTO TeamMembers (Id, Name, CurrentMood, MoodUpdatedAt) VALUES
(1, 'Alice Johnson', NULL, NULL),
(2, 'Bob Smith', NULL, NULL),
(3, 'Charlie Davis', NULL, NULL),
(4, 'Diana Chen', NULL, NULL);
```

### Migration Execution

Run during application startup or via CLI tool:

```csharp
// In Program.cs or startup configuration
using var connection = dbConnectionFactory.CreateConnection();
connection.Open();

var migrationScript = File.ReadAllText("Data/Migrations/001_InitialSchema.sql");
connection.Execute(migrationScript);
```

---

## Query Patterns

### Common Queries with Dapper

#### Get All Team Members with Goals (US1)

```csharp
// Multi-mapping query to hydrate TeamMember.Goals collection
var query = @"
    SELECT
        tm.Id, tm.Name, tm.CurrentMood, tm.MoodUpdatedAt,
        g.Id, g.TeamMemberId, g.GoalText, g.CreatedAt, g.IsCompleted
    FROM TeamMembers tm
    LEFT JOIN Goals g ON tm.Id = g.TeamMemberId
    ORDER BY tm.Name, g.CreatedAt DESC
";

var teamMemberDict = new Dictionary<int, TeamMember>();

var result = connection.Query<TeamMember, Goal, TeamMember>(
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
```

#### Get Team Member by ID (US0)

```csharp
var query = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers WHERE Id = @Id";
var teamMember = connection.QuerySingleOrDefault<TeamMember>(query, new { Id = id });
```

#### Insert New Goal (US2)

```csharp
var query = @"
    INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
    VALUES (@TeamMemberId, @GoalText, @CreatedAt, 0);

    SELECT last_insert_rowid();
";

var newGoalId = connection.QuerySingle<int>(query, new
{
    TeamMemberId = goal.TeamMemberId,
    GoalText = goal.GoalText,
    CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
});
```

#### Update Team Member Mood (US3)

```csharp
var query = @"
    UPDATE TeamMembers
    SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp
    WHERE Id = @Id
";

var rowsAffected = connection.Execute(query, new
{
    Id = id,
    Mood = mood.ToString(),
    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
});
```

#### Toggle Goal Completion (US2)

```csharp
var query = @"
    UPDATE Goals
    SET IsCompleted = CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END
    WHERE Id = @Id
";

var rowsAffected = connection.Execute(query, new { Id = goalId });
```

#### Get Daily Statistics (US4)

```csharp
var statsQuery = @"
    -- Total goals created today
    SELECT COUNT(*)
    FROM Goals
    WHERE DATE(CreatedAt) = DATE('now');

    -- Completed goals today
    SELECT COUNT(*)
    FROM Goals
    WHERE DATE(CreatedAt) = DATE('now') AND IsCompleted = 1;

    -- Mood breakdown
    SELECT CurrentMood, COUNT(*) as Count
    FROM TeamMembers
    WHERE CurrentMood IS NOT NULL
    GROUP BY CurrentMood;
";

using var multi = connection.QueryMultiple(statsQuery);
var totalGoals = multi.ReadSingle<int>();
var completedGoals = multi.ReadSingle<int>();
var moodBreakdown = multi.Read<MoodCount>().ToList();
```

#### Delete Goal (US2)

```csharp
var query = "DELETE FROM Goals WHERE Id = @Id";
var rowsAffected = connection.Execute(query, new { Id = goalId });
```

---

## C# Model Classes

### TeamMember.cs

```csharp
namespace TeamDailyGoalTracker.Models;

public class TeamMember
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Mood? CurrentMood { get; set; }
    public DateTime? MoodUpdatedAt { get; set; }

    // Navigation property (not stored in DB, populated via Dapper multi-mapping)
    public List<Goal> Goals { get; set; } = new();
}

public enum Mood
{
    Great,
    Good,
    Okay,
    Struggling,
    Overwhelmed
}
```

### Goal.cs

```csharp
namespace TeamDailyGoalTracker.Models;

public class Goal
{
    public int Id { get; set; }
    public int TeamMemberId { get; set; }
    public string GoalText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }

    // Navigation property (optional, not used in MVP)
    public TeamMember? TeamMember { get; set; }
}
```

---

## TypeScript Interfaces (Frontend)

### types/TeamMember.ts

```typescript
export enum Mood {
  Great = 'Great',
  Good = 'Good',
  Okay = 'Okay',
  Struggling = 'Struggling',
  Overwhelmed = 'Overwhelmed'
}

export interface TeamMember {
  id: number;
  name: string;
  currentMood: Mood | null;
  moodUpdatedAt: string | null; // ISO 8601 string
  goals: Goal[];
}
```

### types/Goal.ts

```typescript
export interface Goal {
  id: number;
  teamMemberId: number;
  goalText: string;
  createdAt: string; // ISO 8601 string
  isCompleted: boolean;
}
```

---

## Data Validation

### Database-Level Validation (SQLite CHECK Constraints)

- **Name length**: ≤ 100 characters (enforced by `CHECK(length(Name) <= 100)`)
- **Goal text length**: ≤ 500 characters (enforced by `CHECK(length(GoalText) <= 500)`)
- **Mood enum**: Only valid mood values (enforced by `CHECK(CurrentMood IN (...))`)
- **IsCompleted**: Only 0 or 1 (enforced by `CHECK(IsCompleted IN (0, 1))`)

### Application-Level Validation (C# Data Annotations)

```csharp
// DTOs/GoalCreateRequest.cs
public class GoalCreateRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TeamMemberId must be a positive integer")]
    public int TeamMemberId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "GoalText must be between 1 and 500 characters")]
    public string GoalText { get; set; } = string.Empty;
}

// DTOs/MoodUpdateRequest.cs
public class MoodUpdateRequest
{
    [Required]
    [EnumDataType(typeof(Mood), ErrorMessage = "Invalid mood value")]
    public Mood Mood { get; set; }
}
```

### Frontend Validation (Vue + TypeScript)

```typescript
// In GoalInputForm.vue
const goalText = ref('');
const errorMessage = ref('');

const validateGoalText = (): boolean => {
  if (goalText.value.trim().length === 0) {
    errorMessage.value = 'Goal text cannot be empty';
    return false;
  }
  if (goalText.value.length > 500) {
    errorMessage.value = 'Goal text must be 500 characters or less';
    return false;
  }
  errorMessage.value = '';
  return true;
};
```

---

## Performance Considerations

### Index Strategy
- **idx_goals_team_member**: Optimizes `WHERE TeamMemberId = ?` queries (used in dashboard view)
- **idx_goals_created_at**: Optimizes date-based filtering for statistics (e.g., goals created today)

### Query Optimization
- **Use LEFT JOIN**: Dashboard query uses LEFT JOIN to include team members with zero goals
- **Limit result sets**: Not needed for MVP (max 20 team members × ~5 goals each = ~100 rows)
- **Avoid N+1**: Use Dapper multi-mapping to fetch team members + goals in single query

### Expected Performance
- **Table sizes**: TeamMembers ~20 rows, Goals ~100-500 rows (daily reset assumed)
- **Query latency**: < 10ms for all queries (SQLite is in-memory fast for small datasets)
- **Concurrency**: SQLite write serialization acceptable for team size ≤20

---

## Future Enhancements (Out of Scope for MVP)

- **History table**: Store historical moods/goals with timestamps
- **Soft deletes**: Add `DeletedAt` column instead of hard deletes
- **Audit logging**: Track who changed what and when
- **Goal categories**: Add `Category` column to Goals (e.g., "Development", "Review", "Meeting")
- **Comments**: Add `Comments` table for goal discussions
- **Attachments**: Add `Attachments` table for file uploads

---

## Schema Verification

### Check Tables Exist

```sql
SELECT name FROM sqlite_master WHERE type='table';
-- Expected: TeamMembers, Goals
```

### Check Indexes Exist

```sql
SELECT name FROM sqlite_master WHERE type='index';
-- Expected: idx_goals_team_member, idx_goals_created_at
```

### Verify Foreign Key Constraints

```sql
PRAGMA foreign_keys;
-- Expected: 1 (enabled)

PRAGMA foreign_key_list(Goals);
-- Expected: Shows FK from TeamMemberId to TeamMembers(Id)
```

### Test Referential Integrity

```sql
-- This should fail (FK violation)
INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
VALUES (9999, 'Invalid goal', '2025-11-20 10:00:00', 0);
-- Expected: FOREIGN KEY constraint failed

-- This should cascade delete goals
DELETE FROM TeamMembers WHERE Id = 1;
-- Expected: All goals with TeamMemberId = 1 are deleted automatically
```

---

## Summary

The data model is intentionally minimal to support MVP requirements:
- **2 tables**: TeamMembers, Goals
- **1 relationship**: One-to-Many (TeamMembers → Goals)
- **3 indexes**: Primary keys (auto) + 2 custom indexes for query performance
- **Normalized**: 3NF (no redundant data)
- **Constrained**: Database-level validation via CHECK constraints
- **Performant**: Optimized for small team sizes (≤20 members)

This design aligns with the constitution principle of **Minimal Full-Stack Architecture** by keeping the data model as simple as possible while meeting all functional requirements.
