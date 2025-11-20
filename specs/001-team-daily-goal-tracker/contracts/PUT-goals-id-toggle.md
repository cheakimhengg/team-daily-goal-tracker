# PUT /api/goals/{id}/toggle

**User Story**: US2 (Set Goals)
**Purpose**: Toggle completion status of a goal (mark complete/incomplete)

---

## Request

### HTTP Method
```
PUT /api/goals/{id}/toggle
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | Unique identifier of the goal to toggle |

### Headers
```
Content-Type: application/json
```

### Request Body
```json
{}
```
*(Empty body - toggle action is idempotent based on current state)*

### Request Example

```http
PUT /api/goals/42/toggle HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{}
```

---

## Response

### Success Response (200 OK)

```json
{
  "data": {
    "id": 42,
    "teamMemberId": 1,
    "goalText": "Review PR #245 for authentication module",
    "createdAt": "2025-11-20T10:30:00Z",
    "isCompleted": true
  }
}
```

### Response Schema

#### GoalResponse

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Unique identifier |
| `teamMemberId` | integer | No | ID of team member who owns this goal |
| `goalText` | string | No | Goal description |
| `createdAt` | string (ISO 8601) | No | UTC timestamp when goal was created |
| `isCompleted` | boolean | No | **Updated** completion status (toggled from previous state) |

---

## Error Responses

### 404 Not Found - Goal Not Found

**Scenario**: Goal with specified `id` does not exist

```json
{
  "error": {
    "code": "GOAL_NOT_FOUND",
    "message": "Goal with ID 999 does not exist"
  }
}
```

### 400 Bad Request - Invalid ID

**Scenario**: `id` parameter is not a valid integer

```json
{
  "error": {
    "code": "INVALID_ID",
    "message": "Goal ID must be a positive integer"
  }
}
```

### 500 Internal Server Error

**Scenario**: Database connection failure or unexpected server error

```json
{
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred while updating the goal"
  }
}
```

---

## Business Rules

1. **Toggle Logic**:
   - If `isCompleted = false` → set to `true`
   - If `isCompleted = true` → set to `false`
2. **Idempotent**: Multiple calls with same ID toggle the state repeatedly (not idempotent in REST sense, but predictable)
3. **No Validation**: No request body validation (empty body accepted)
4. **Optimistic Update**: Frontend should update UI immediately, rollback on error

---

## Behavior Examples

| Current State | After Toggle | Description |
|--------------|--------------|-------------|
| `isCompleted: false` | `isCompleted: true` | Marks goal as complete |
| `isCompleted: true` | `isCompleted: false` | Marks goal as incomplete (undo) |

---

## Performance

- **Expected Response Time**: < 30ms
- **Concurrency**: SQLite write serialization (safe for concurrent toggles)
- **Race Condition**: Last write wins (acceptable for MVP)

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function toggleGoalCompletion(goalId: number): Promise<Goal> {
  const response = await fetch(`http://localhost:5000/api/goals/${goalId}/toggle`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({})
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.error.message || 'Failed to toggle goal');
  }

  const json = await response.json();
  return json.data;
}
```

### Component Usage (Optimistic Update)

```vue
<script setup lang="ts">
import { ref } from 'vue';
import { toggleGoalCompletion } from '@/services/api';
import type { Goal } from '@/types/Goal';

const props = defineProps<{ goal: Goal }>();
const emit = defineEmits<{ 'goal-updated': [Goal] }>();

const isToggling = ref(false);

const handleToggle = async () => {
  // Optimistic update
  const previousState = props.goal.isCompleted;
  props.goal.isCompleted = !previousState;

  isToggling.value = true;

  try {
    const updatedGoal = await toggleGoalCompletion(props.goal.id);
    emit('goal-updated', updatedGoal);
  } catch (error) {
    // Rollback on error
    props.goal.isCompleted = previousState;
    alert('Failed to update goal: ' + error.message);
  } finally {
    isToggling.value = false;
  }
};
</script>

<template>
  <div class="flex items-center gap-2">
    <input
      type="checkbox"
      :checked="goal.isCompleted"
      @change="handleToggle"
      :disabled="isToggling"
      class="checkbox checkbox-primary"
    />
    <span :class="{ 'line-through text-gray-400': goal.isCompleted }">
      {{ goal.goalText }}
    </span>
  </div>
</template>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
[HttpPut("{id}/toggle")]
public async Task<IActionResult> ToggleCompletion(int id)
{
    if (id <= 0)
    {
        return BadRequest(new
        {
            error = new
            {
                code = "INVALID_ID",
                message = "Goal ID must be a positive integer"
            }
        });
    }

    try
    {
        var updatedGoal = await _goalService.ToggleCompletionAsync(id);
        return Ok(new { data = updatedGoal });
    }
    catch (GoalNotFoundException ex)
    {
        return NotFound(new
        {
            error = new
            {
                code = "GOAL_NOT_FOUND",
                message = ex.Message
            }
        });
    }
}
```

### Service Layer

```csharp
public async Task<Goal> ToggleCompletionAsync(int goalId)
{
    // Toggle in database
    await _goalRepository.ToggleCompletionAsync(goalId);

    // Fetch updated goal
    var goal = await _goalRepository.GetByIdAsync(goalId);
    if (goal == null)
    {
        throw new GoalNotFoundException($"Goal with ID {goalId} does not exist");
    }

    return goal;
}
```

### Dapper Update Query

```csharp
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
```

---

## Testing

### Test Cases

1. **TC-01**: Toggle goal from incomplete to complete → Returns 200 with `isCompleted: true`
2. **TC-02**: Toggle goal from complete to incomplete → Returns 200 with `isCompleted: false`
3. **TC-03**: Toggle twice → Returns to original state
4. **TC-04**: Invalid goal ID → Returns 404
5. **TC-05**: Negative or zero ID → Returns 400
6. **TC-06**: Response includes all goal fields (not just `isCompleted`)

### Contract Test Example (xUnit)

```csharp
[Fact]
public async Task ToggleCompletion_WithValidId_TogglesIsCompleted()
{
    // Arrange: Create a goal (isCompleted = false by default)
    var goal = await CreateTestGoal();

    // Act: Toggle to completed
    var response1 = await _client.PutAsync($"/api/goals/{goal.Id}/toggle", null);
    var json1 = await response1.Content.ReadAsStringAsync();
    var result1 = JsonSerializer.Deserialize<GoalResponse>(json1);

    // Assert: Now completed
    Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
    Assert.True(result1.Data.IsCompleted);

    // Act: Toggle back to incomplete
    var response2 = await _client.PutAsync($"/api/goals/{goal.Id}/toggle", null);
    var json2 = await response2.Content.ReadAsStringAsync();
    var result2 = JsonSerializer.Deserialize<GoalResponse>(json2);

    // Assert: Now incomplete
    Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    Assert.False(result2.Data.IsCompleted);
}

[Fact]
public async Task ToggleCompletion_WithInvalidId_Returns404()
{
    // Act
    var response = await _client.PutAsync("/api/goals/9999/toggle", null);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

## Alternative Design (Not Implemented)

**Explicit PUT /api/goals/{id}** with request body:
```json
{
  "isCompleted": true
}
```

**Why Toggle is Preferred for MVP**:
- Simpler frontend logic (no need to track current state)
- Fewer opportunities for desync (current state is in database, not client)
- Single button click for both complete/incomplete actions
- Matches user mental model (checkbox toggle)

---

## Security Considerations

- **Authorization**: Not implemented in MVP (any user can toggle any goal)
- **SQL Injection**: Prevented by Dapper parameterized queries
- **Concurrent Updates**: Last write wins (acceptable for MVP)

---

## Future Enhancements (Out of Scope)

- Add `completedAt` timestamp field to track when goal was marked complete
- Add `completedBy` field for multi-user environments
- Add optimistic concurrency check (return 409 Conflict if state changed between read and write)
- Add audit log for goal state changes
