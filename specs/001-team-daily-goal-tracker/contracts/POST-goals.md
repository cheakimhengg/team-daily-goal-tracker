# POST /api/goals

**User Story**: US2 (Set Goals)
**Purpose**: Create a new goal for a team member

---

## Request

### HTTP Method
```
POST /api/goals
```

### Headers
```
Content-Type: application/json
```

### Request Body

```json
{
  "teamMemberId": 1,
  "goalText": "Review PR #245 for authentication module"
}
```

### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `teamMemberId` | integer | Yes | Must be > 0 and reference existing team member | ID of team member creating the goal |
| `goalText` | string | Yes | Length: 1-500 characters | Goal description |

### Request Example

```http
POST /api/goals HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "teamMemberId": 1,
  "goalText": "Review PR #245 for authentication module"
}
```

---

## Response

### Success Response (201 Created)

```json
{
  "data": {
    "id": 42,
    "teamMemberId": 1,
    "goalText": "Review PR #245 for authentication module",
    "createdAt": "2025-11-20T10:30:00Z",
    "isCompleted": false
  }
}
```

### Response Schema

#### GoalResponse

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Unique identifier of newly created goal |
| `teamMemberId` | integer | No | ID of team member who owns this goal |
| `goalText` | string | No | Goal description (as submitted) |
| `createdAt` | string (ISO 8601) | No | UTC timestamp when goal was created |
| `isCompleted` | boolean | No | Always `false` for new goals |

### Response Headers
```
Location: /api/goals/42
```

---

## Error Responses

### 400 Bad Request - Validation Errors

**Scenario**: Missing required fields or validation failures

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": {
      "teamMemberId": ["TeamMemberId must be a positive integer"],
      "goalText": ["GoalText must be between 1 and 500 characters"]
    }
  }
}
```

**Common Validation Errors**:
- `teamMemberId` is missing, zero, or negative
- `goalText` is missing, empty, or > 500 characters

### 404 Not Found - Team Member Not Found

**Scenario**: `teamMemberId` does not reference an existing team member

```json
{
  "error": {
    "code": "TEAM_MEMBER_NOT_FOUND",
    "message": "Team member with ID 999 does not exist"
  }
}
```

### 500 Internal Server Error

**Scenario**: Database connection failure or unexpected server error

```json
{
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred while creating the goal"
  }
}
```

---

## Business Rules

1. **Auto-generate ID**: Backend assigns unique `id` automatically (SQLite AUTOINCREMENT)
2. **Auto-set timestamp**: Backend sets `createdAt` to UTC timestamp at moment of creation
3. **Default completion**: All new goals have `isCompleted = false`
4. **Team member validation**: Must verify `teamMemberId` exists before inserting goal
5. **Text trimming**: Frontend should trim whitespace from `goalText` before sending

---

## Validation Rules

### Client-Side (Frontend)
- Check `goalText` length (1-500 chars) before submit
- Show character counter (e.g., "245/500")
- Trim leading/trailing whitespace
- Disable submit button if validation fails

### Server-Side (Backend)
- **teamMemberId**: Required, must be > 0, must exist in `TeamMembers` table
- **goalText**: Required, length 1-500 characters
- Return 400 with detailed error messages if validation fails

---

## Performance

- **Expected Response Time**: < 50ms
- **Concurrency**: SQLite handles write serialization automatically
- **Rate Limiting**: Not implemented in MVP

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function createGoal(teamMemberId: number, goalText: string): Promise<Goal> {
  const response = await fetch('http://localhost:5000/api/goals', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ teamMemberId, goalText: goalText.trim() })
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.error.message || 'Failed to create goal');
  }

  const json = await response.json();
  return json.data;
}
```

### Component Usage

```vue
<script setup lang="ts">
import { ref } from 'vue';
import { createGoal } from '@/services/api';

const goalText = ref('');
const errorMessage = ref('');

const handleSubmit = async () => {
  if (goalText.value.trim().length === 0 || goalText.value.length > 500) {
    errorMessage.value = 'Goal text must be between 1 and 500 characters';
    return;
  }

  try {
    const newGoal = await createGoal(props.teamMemberId, goalText.value);
    emit('goal-created', newGoal);
    goalText.value = ''; // Clear form
    errorMessage.value = '';
  } catch (error) {
    errorMessage.value = error.message;
  }
};
</script>

<template>
  <form @submit.prevent="handleSubmit">
    <textarea
      v-model="goalText"
      maxlength="500"
      placeholder="What do you want to accomplish today?"
      class="textarea textarea-bordered"
    />
    <p class="text-sm text-gray-500">{{ goalText.length }}/500</p>
    <button type="submit" class="btn btn-primary">Add Goal</button>
    <p v-if="errorMessage" class="text-error">{{ errorMessage }}</p>
  </form>
</template>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] GoalCreateRequest request)
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
        var goal = await _goalService.CreateAsync(request.TeamMemberId, request.GoalText);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, new { data = goal });
    }
    catch (TeamMemberNotFoundException ex)
    {
        return NotFound(new
        {
            error = new
            {
                code = "TEAM_MEMBER_NOT_FOUND",
                message = ex.Message
            }
        });
    }
}
```

### Request DTO

```csharp
public class GoalCreateRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TeamMemberId must be a positive integer")]
    public int TeamMemberId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "GoalText must be between 1 and 500 characters")]
    public string GoalText { get; set; } = string.Empty;
}
```

### Dapper Insert Query

```csharp
var query = @"
    INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
    VALUES (@TeamMemberId, @GoalText, @CreatedAt, 0);

    SELECT last_insert_rowid();
";

var newGoalId = await connection.QuerySingleAsync<int>(query, new
{
    TeamMemberId = teamMemberId,
    GoalText = goalText,
    CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
});
```

---

## Testing

### Test Cases

1. **TC-01**: Valid request → Returns 201 with created goal (includes generated ID)
2. **TC-02**: Missing `teamMemberId` → Returns 400 with validation error
3. **TC-03**: Empty `goalText` → Returns 400 with validation error
4. **TC-04**: `goalText` > 500 chars → Returns 400 with validation error
5. **TC-05**: Invalid `teamMemberId` (doesn't exist) → Returns 404
6. **TC-06**: New goal has `isCompleted = false` by default
7. **TC-07**: Response includes `Location` header with new goal URL

### Contract Test Example (xUnit)

```csharp
[Fact]
public async Task Create_WithValidRequest_Returns201WithCreatedGoal()
{
    // Arrange
    var request = new { teamMemberId = 1, goalText = "Test goal" };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync("/api/goals", content);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<GoalCreateResponse>(json);

    Assert.NotNull(result.Data);
    Assert.True(result.Data.Id > 0);
    Assert.Equal(1, result.Data.TeamMemberId);
    Assert.Equal("Test goal", result.Data.GoalText);
    Assert.False(result.Data.IsCompleted);
    Assert.NotNull(result.Data.CreatedAt);
}

[Fact]
public async Task Create_WithInvalidTeamMemberId_Returns404()
{
    // Arrange
    var request = new { teamMemberId = 9999, goalText = "Test goal" };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync("/api/goals", content);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

## Security Considerations

- **SQL Injection**: Prevented by Dapper parameterized queries
- **XSS**: Frontend (Vue) auto-escapes HTML in `goalText` display
- **Input Validation**: Both client and server validate input length
- **Authorization**: Not implemented in MVP (no user authentication)
