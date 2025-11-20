# GET /api/team-members

**User Story**: US0 (Identify Self), US1 (View Dashboard)
**Purpose**: Retrieve all team members with optional goal inclusion for dashboard display

---

## Request

### HTTP Method
```
GET /api/team-members
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `includeGoals` | boolean | No | `false` | If `true`, includes each team member's goals in response |

### Headers
```
Content-Type: application/json
```

### Request Example

```http
GET /api/team-members?includeGoals=true HTTP/1.1
Host: localhost:5000
Content-Type: application/json
```

---

## Response

### Success Response (200 OK)

#### Without Goals (`includeGoals=false`)

```json
{
  "data": [
    {
      "id": 1,
      "name": "Alice Johnson",
      "currentMood": "Great",
      "moodUpdatedAt": "2025-11-20T09:15:00Z"
    },
    {
      "id": 2,
      "name": "Bob Smith",
      "currentMood": "Good",
      "moodUpdatedAt": "2025-11-20T09:20:00Z"
    },
    {
      "id": 3,
      "name": "Charlie Davis",
      "currentMood": null,
      "moodUpdatedAt": null
    }
  ]
}
```

#### With Goals (`includeGoals=true`)

```json
{
  "data": [
    {
      "id": 1,
      "name": "Alice Johnson",
      "currentMood": "Great",
      "moodUpdatedAt": "2025-11-20T09:15:00Z",
      "goals": [
        {
          "id": 1,
          "teamMemberId": 1,
          "goalText": "Review PR #245 for authentication module",
          "createdAt": "2025-11-20T10:00:00Z",
          "isCompleted": false
        },
        {
          "id": 2,
          "teamMemberId": 1,
          "goalText": "Update API documentation for new endpoints",
          "createdAt": "2025-11-20T10:05:00Z",
          "isCompleted": true
        }
      ]
    },
    {
      "id": 2,
      "name": "Bob Smith",
      "currentMood": "Good",
      "moodUpdatedAt": "2025-11-20T09:20:00Z",
      "goals": []
    }
  ]
}
```

### Response Schema

#### TeamMemberResponse

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Unique identifier |
| `name` | string | No | Team member's display name |
| `currentMood` | string (enum) | Yes | Current mood: `"Great"`, `"Good"`, `"Okay"`, `"Struggling"`, `"Overwhelmed"`, or `null` |
| `moodUpdatedAt` | string (ISO 8601) | Yes | UTC timestamp of last mood update, or `null` if never set |
| `goals` | array of GoalResponse | Yes (only if `includeGoals=true`) | List of team member's goals |

#### GoalResponse (nested in TeamMemberResponse)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Unique identifier |
| `teamMemberId` | integer | No | ID of team member who owns this goal |
| `goalText` | string | No | Goal description (max 500 chars) |
| `createdAt` | string (ISO 8601) | No | UTC timestamp when goal was created |
| `isCompleted` | boolean | No | Completion status: `true` or `false` |

---

## Error Responses

### 500 Internal Server Error

**Scenario**: Database connection failure or unexpected server error

```json
{
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred while retrieving team members"
  }
}
```

---

## Business Rules

1. **Sorting**: Team members are returned sorted alphabetically by `name`
2. **Goal Sorting**: When `includeGoals=true`, goals are sorted by `createdAt` descending (newest first)
3. **Empty Goals**: Team members with no goals return `"goals": []` when `includeGoals=true`
4. **Null Moods**: Team members who haven't set a mood return `"currentMood": null` and `"moodUpdatedAt": null`

---

## Performance

- **Expected Response Time**: < 50ms for 20 team members with 5 goals each
- **Caching**: No caching (data changes frequently)
- **Pagination**: Not implemented in MVP (dataset is small)

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function getTeamMembers(includeGoals: boolean = false): Promise<TeamMember[]> {
  const response = await fetch(
    `http://localhost:5000/api/team-members?includeGoals=${includeGoals}`,
    {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' }
    }
  );

  if (!response.ok) {
    throw new Error('Failed to fetch team members');
  }

  const json = await response.json();
  return json.data;
}
```

### Component Usage

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getTeamMembers } from '@/services/api';
import type { TeamMember } from '@/types/TeamMember';

const teamMembers = ref<TeamMember[]>([]);

onMounted(async () => {
  teamMembers.value = await getTeamMembers(true); // Include goals for dashboard
});
</script>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] bool includeGoals = false)
{
    var teamMembers = await _teamMemberService.GetAllAsync(includeGoals);
    return Ok(new { data = teamMembers });
}
```

### Dapper Query (with goals)

```csharp
var query = @"
    SELECT
        tm.Id, tm.Name, tm.CurrentMood, tm.MoodUpdatedAt,
        g.Id, g.TeamMemberId, g.GoalText, g.CreatedAt, g.IsCompleted
    FROM TeamMembers tm
    LEFT JOIN Goals g ON tm.Id = g.TeamMemberId
    ORDER BY tm.Name, g.CreatedAt DESC
";
```

---

## Testing

### Test Cases

1. **TC-01**: GET without `includeGoals` → Returns team members without `goals` property
2. **TC-02**: GET with `includeGoals=true` → Returns team members with nested `goals` array
3. **TC-03**: Team member with no goals → Returns `"goals": []`
4. **TC-04**: Team member with null mood → Returns `"currentMood": null, "moodUpdatedAt": null`
5. **TC-05**: Response sorting → Team members sorted alphabetically, goals sorted by `createdAt` DESC

### Contract Test Example (xUnit)

```csharp
[Fact]
public async Task GetAll_WithIncludeGoals_ReturnsTeamMembersWithGoals()
{
    // Arrange
    var response = await _client.GetAsync("/api/team-members?includeGoals=true");

    // Assert
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<TeamMembersResponse>(json);

    Assert.NotNull(result.Data);
    Assert.All(result.Data, tm => Assert.NotNull(tm.Goals));
}
```
