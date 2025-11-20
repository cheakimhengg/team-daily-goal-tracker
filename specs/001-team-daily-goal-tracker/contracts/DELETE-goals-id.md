# DELETE /api/goals/{id}

**User Story**: US2 (Set Goals)
**Purpose**: Delete a goal permanently

---

## Request

### HTTP Method
```
DELETE /api/goals/{id}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | Unique identifier of the goal to delete |

### Headers
```
Content-Type: application/json
```

### Request Body
*(No body required)*

### Request Example

```http
DELETE /api/goals/42 HTTP/1.1
Host: localhost:5000
Content-Type: application/json
```

---

## Response

### Success Response (204 No Content)

**Status Code**: `204 No Content`

**Body**: *(Empty - no response body on successful deletion)*

---

## Error Responses

### 404 Not Found - Goal Not Found

**Scenario**: Goal with specified `id` does not exist (or already deleted)

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
    "message": "An unexpected error occurred while deleting the goal"
  }
}
```

---

## Business Rules

1. **Hard Delete**: Goal is permanently removed from database (no soft delete in MVP)
2. **Idempotent**: DELETE on non-existent goal returns 404 (not 204)
3. **No Confirmation**: API does not require confirmation (frontend should handle)
4. **Cascade**: No cascade effects (goals have no dependent records in MVP)

---

## Performance

- **Expected Response Time**: < 30ms
- **Concurrency**: Safe for concurrent deletes (SQLite write serialization)
- **Race Condition**: If two users delete same goal simultaneously, first succeeds (204), second gets 404

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function deleteGoal(goalId: number): Promise<void> {
  const response = await fetch(`http://localhost:5000/api/goals/${goalId}`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' }
  });

  if (response.status === 404) {
    throw new Error('Goal not found or already deleted');
  }

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.error.message || 'Failed to delete goal');
  }

  // Success - no response body to parse (204 No Content)
}
```

### Component Usage (with Confirmation)

```vue
<script setup lang="ts">
import { ref } from 'vue';
import { deleteGoal } from '@/services/api';
import type { Goal } from '@/types/Goal';

const props = defineProps<{ goal: Goal }>();
const emit = defineEmits<{ 'goal-deleted': [number] }>();

const isDeleting = ref(false);

const handleDelete = async () => {
  // Frontend confirmation dialog
  if (!confirm(`Delete goal: "${props.goal.goalText}"?`)) {
    return;
  }

  isDeleting.value = true;

  try {
    await deleteGoal(props.goal.id);
    emit('goal-deleted', props.goal.id); // Notify parent to remove from UI
  } catch (error) {
    alert('Failed to delete goal: ' + error.message);
  } finally {
    isDeleting.value = false;
  }
};
</script>

<template>
  <button
    @click="handleDelete"
    :disabled="isDeleting"
    class="btn btn-error btn-sm"
  >
    {{ isDeleting ? 'Deleting...' : 'Delete' }}
  </button>
</template>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
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
        await _goalService.DeleteAsync(id);
        return NoContent(); // 204 No Content
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
public async Task DeleteAsync(int goalId)
{
    var deleted = await _goalRepository.DeleteAsync(goalId);

    if (!deleted)
    {
        throw new GoalNotFoundException($"Goal with ID {goalId} does not exist");
    }
}
```

### Dapper Delete Query

```csharp
public async Task<bool> DeleteAsync(int goalId)
{
    var query = "DELETE FROM Goals WHERE Id = @Id";
    var rowsAffected = await _connection.ExecuteAsync(query, new { Id = goalId });

    return rowsAffected > 0;
}
```

---

## Testing

### Test Cases

1. **TC-01**: Delete existing goal → Returns 204 No Content
2. **TC-02**: Delete non-existent goal → Returns 404
3. **TC-03**: Delete with negative ID → Returns 400
4. **TC-04**: Delete twice (idempotency) → First returns 204, second returns 404
5. **TC-05**: Verify goal removed from database after 204 response
6. **TC-06**: Delete does not affect other goals or team members

### Contract Test Example (xUnit)

```csharp
[Fact]
public async Task Delete_WithValidId_Returns204()
{
    // Arrange: Create a test goal
    var goal = await CreateTestGoal();

    // Act
    var response = await _client.DeleteAsync($"/api/goals/{goal.Id}");

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    // Verify goal is actually deleted
    var getResponse = await _client.GetAsync($"/api/goals/{goal.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
}

[Fact]
public async Task Delete_WithInvalidId_Returns404()
{
    // Act
    var response = await _client.DeleteAsync("/api/goals/9999");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}

[Fact]
public async Task Delete_Twice_SecondReturns404()
{
    // Arrange: Create a test goal
    var goal = await CreateTestGoal();

    // Act: First delete
    var response1 = await _client.DeleteAsync($"/api/goals/{goal.Id}");
    Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

    // Act: Second delete (idempotency test)
    var response2 = await _client.DeleteAsync($"/api/goals/{goal.Id}");

    // Assert: Should return 404 (not 204, as goal no longer exists)
    Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
}
```

---

## UI/UX Considerations

### Confirmation Dialog
Frontend should show confirmation before deleting:
```
"Are you sure you want to delete this goal?"
[Goal text preview]
[Cancel] [Delete]
```

### Visual Feedback
1. Show loading state during deletion (disable button, show spinner)
2. Optimistic removal from UI (remove from list immediately, rollback on error)
3. Show success toast: "Goal deleted successfully"
4. Show error toast on failure: "Failed to delete goal. Please try again."

### Undo Functionality (Out of Scope for MVP)
- Add "Undo" button in success toast
- Store deleted goal temporarily (5 seconds)
- Allow re-insertion if user clicks "Undo"

---

## Security Considerations

- **Authorization**: Not implemented in MVP (any user can delete any goal)
- **SQL Injection**: Prevented by Dapper parameterized queries
- **Accidental Deletion**: Frontend confirmation dialog mitigates risk
- **Audit Trail**: Not implemented in MVP (no record of who deleted what)

---

## REST Compliance

### Why 204 (No Content) instead of 200 (OK)?
- **REST Standard**: DELETE should return 204 when successful with no response body
- **200 OK**: Used when DELETE returns confirmation data in response body
- **For MVP**: 204 is preferred (no need to return deleted goal data)

### Why 404 instead of 204 for Non-Existent Goal?
- **Idempotency Debate**: Some APIs return 204 even if resource doesn't exist (idempotent result)
- **For MVP**: 404 provides clearer feedback to frontend (goal doesn't exist vs. successfully deleted)
- **Practical**: Helps detect bugs (e.g., frontend trying to delete already-deleted goal)

---

## Future Enhancements (Out of Scope)

- **Soft Delete**: Add `DeletedAt` column, mark goals as deleted instead of removing
- **Undo Functionality**: Allow restoration of recently deleted goals
- **Batch Delete**: `DELETE /api/goals?ids=1,2,3` for multiple deletions
- **Authorization**: Only goal owner can delete their own goals
- **Audit Log**: Track who deleted what and when
