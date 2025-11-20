# PUT /api/team-members/{id}/mood

**User Story**: US3 (Update Mood)
**Purpose**: Update a team member's current mood

---

## Request

### HTTP Method
```
PUT /api/team-members/{id}/mood
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | Unique identifier of the team member |

### Headers
```
Content-Type: application/json
```

### Request Body

```json
{
  "mood": "Great"
}
```

### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `mood` | string (enum) | Yes | Must be one of: `"Great"`, `"Good"`, `"Okay"`, `"Struggling"`, `"Overwhelmed"` | New mood value |

### Request Example

```http
PUT /api/team-members/1/mood HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "mood": "Great"
}
```

---

## Response

### Success Response (200 OK)

```json
{
  "data": {
    "id": 1,
    "name": "Alice Johnson",
    "currentMood": "Great",
    "moodUpdatedAt": "2025-11-20T14:30:00Z"
  }
}
```

### Response Schema

#### TeamMemberResponse

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | integer | No | Unique identifier |
| `name` | string | No | Team member's display name |
| `currentMood` | string (enum) | No | **Updated** mood value |
| `moodUpdatedAt` | string (ISO 8601) | No | **Updated** UTC timestamp of this mood change |

---

## Error Responses

### 400 Bad Request - Validation Errors

**Scenario**: Invalid mood value

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": {
      "mood": ["Invalid mood value. Must be one of: Great, Good, Okay, Struggling, Overwhelmed"]
    }
  }
}
```

**Common Validation Errors**:
- `mood` is missing
- `mood` is not one of the valid enum values
- `mood` is empty string or null

### 404 Not Found - Team Member Not Found

**Scenario**: Team member with specified `id` does not exist

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
    "message": "An unexpected error occurred while updating mood"
  }
}
```

---

## Business Rules

1. **Auto-update timestamp**: Backend sets `moodUpdatedAt` to current UTC time automatically
2. **Overwrite previous mood**: Each update replaces the previous mood (no history in MVP)
3. **Valid moods only**: Must be one of 5 valid mood values (enforced by database CHECK constraint and API validation)
4. **Case-sensitive**: Mood values are case-sensitive (`"Great"` is valid, `"great"` is not)

---

## Mood Enumeration

| Mood Value | UI Display | Color (DaisyUI) | Description |
|-----------|------------|-----------------|-------------|
| `"Great"` | 😊 Great | `bg-success` (green) | Feeling excellent, productive |
| `"Good"` | 🙂 Good | `bg-info` (blue) | Feeling positive, on track |
| `"Okay"` | 😐 Okay | `bg-warning` (yellow) | Neutral, managing |
| `"Struggling"` | 😟 Struggling | `bg-warning` (orange) | Facing challenges, need support |
| `"Overwhelmed"` | 😰 Overwhelmed | `bg-error` (red) | Very stressed, need help |

---

## Performance

- **Expected Response Time**: < 30ms
- **Concurrency**: Last write wins (acceptable for MVP - spec acknowledges this)
- **Optimistic Update**: Frontend should update UI immediately, rollback on error

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function updateMood(teamMemberId: number, mood: Mood): Promise<TeamMember> {
  const response = await fetch(`http://localhost:5000/api/team-members/${teamMemberId}/mood`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mood })
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.error.message || 'Failed to update mood');
  }

  const json = await response.json();
  return json.data;
}
```

### Component Usage (Optimistic Update)

```vue
<script setup lang="ts">
import { ref } from 'vue';
import { updateMood } from '@/services/api';
import { Mood } from '@/types/TeamMember';
import type { TeamMember } from '@/types/TeamMember';

const props = defineProps<{ teamMember: TeamMember }>();
const emit = defineEmits<{ 'mood-updated': [TeamMember] }>();

const isUpdating = ref(false);

const handleMoodChange = async (newMood: Mood) => {
  // Optimistic update
  const previousMood = props.teamMember.currentMood;
  const previousTimestamp = props.teamMember.moodUpdatedAt;

  props.teamMember.currentMood = newMood;
  props.teamMember.moodUpdatedAt = new Date().toISOString();

  isUpdating.value = true;

  try {
    const updatedTeamMember = await updateMood(props.teamMember.id, newMood);
    emit('mood-updated', updatedTeamMember);
  } catch (error) {
    // Rollback on error
    props.teamMember.currentMood = previousMood;
    props.teamMember.moodUpdatedAt = previousTimestamp;
    alert('Failed to update mood: ' + error.message);
  } finally {
    isUpdating.value = false;
  }
};
</script>

<template>
  <div class="btn-group">
    <button
      v-for="mood in Object.values(Mood)"
      :key="mood"
      @click="handleMoodChange(mood)"
      :disabled="isUpdating"
      :class="[
        'btn',
        teamMember.currentMood === mood ? 'btn-active' : ''
      ]"
    >
      {{ mood }}
    </button>
  </div>
</template>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
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
public class MoodUpdateRequest
{
    [Required(ErrorMessage = "Mood is required")]
    [EnumDataType(typeof(Mood), ErrorMessage = "Invalid mood value. Must be one of: Great, Good, Okay, Struggling, Overwhelmed")]
    public Mood Mood { get; set; }
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

### Dapper Update Query

```csharp
public async Task<TeamMember> UpdateMoodAsync(int id, Mood mood)
{
    var updateQuery = @"
        UPDATE TeamMembers
        SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp
        WHERE Id = @Id
    ";

    var rowsAffected = await _connection.ExecuteAsync(updateQuery, new
    {
        Id = id,
        Mood = mood.ToString(),
        Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
    });

    if (rowsAffected == 0)
    {
        throw new TeamMemberNotFoundException($"Team member with ID {id} does not exist");
    }

    // Fetch updated team member
    var selectQuery = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers WHERE Id = @Id";
    var teamMember = await _connection.QuerySingleAsync<TeamMember>(selectQuery, new { Id = id });

    return teamMember;
}
```

---

## Testing

### Test Cases

1. **TC-01**: Update mood with valid value → Returns 200 with updated team member
2. **TC-02**: Update mood to same value → Returns 200 (idempotent, updates timestamp)
3. **TC-03**: Update mood with invalid enum value → Returns 400 with validation error
4. **TC-04**: Update mood for non-existent team member → Returns 404
5. **TC-05**: Response includes updated `moodUpdatedAt` timestamp
6. **TC-06**: Test all 5 valid mood values
7. **TC-07**: Missing `mood` field in request → Returns 400

### Contract Test Example (xUnit)

```csharp
[Theory]
[InlineData(Mood.Great)]
[InlineData(Mood.Good)]
[InlineData(Mood.Okay)]
[InlineData(Mood.Struggling)]
[InlineData(Mood.Overwhelmed)]
public async Task UpdateMood_WithValidMood_Returns200(Mood mood)
{
    // Arrange
    var teamMember = await CreateTestTeamMember();
    var request = new { mood = mood.ToString() };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PutAsync($"/api/team-members/{teamMember.Id}/mood", content);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<TeamMemberResponse>(json);

    Assert.Equal(mood.ToString(), result.Data.CurrentMood);
    Assert.NotNull(result.Data.MoodUpdatedAt);
}

[Fact]
public async Task UpdateMood_WithInvalidMood_Returns400()
{
    // Arrange
    var teamMember = await CreateTestTeamMember();
    var request = new { mood = "InvalidMood" };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PutAsync($"/api/team-members/{teamMember.Id}/mood", content);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task UpdateMood_WithInvalidId_Returns404()
{
    // Arrange
    var request = new { mood = "Great" };
    var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PutAsync("/api/team-members/9999/mood", content);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

## UI/UX Considerations

### Mood Selector Component

**Option 1: Button Group (Recommended)**
```
[😊 Great] [🙂 Good] [😐 Okay] [😟 Struggling] [😰 Overwhelmed]
```
- Current mood button is highlighted (active state)
- Click different button to change mood
- Visual feedback with DaisyUI colors

**Option 2: Dropdown (Alternative)**
```
Current Mood: [Great ▼]
```
- Compact, takes less space
- Less visible for team awareness
- Requires extra click

**Recommended**: Button group for better visibility and one-click updates

### Visual Feedback
1. **Card Background Color**: Change team member card background based on mood
   - `Great` → `bg-success/10` (light green)
   - `Overwhelmed` → `bg-error/10` (light red)
2. **Timestamp Display**: Show "Updated 2 minutes ago" below mood selector
3. **Loading State**: Disable buttons during update, show spinner

---

## Security Considerations

- **Authorization**: Not implemented in MVP (any user can update any team member's mood)
- **SQL Injection**: Prevented by Dapper parameterized queries
- **Enum Validation**: Enforced at both API and database level (CHECK constraint)
- **Concurrent Updates**: Last write wins (acceptable per spec)

---

## Accessibility

- Use semantic HTML buttons for mood selector
- Include ARIA labels: `aria-label="Set mood to Great"`
- Keyboard navigation: Tab through mood buttons, Enter to select
- Screen reader announcement: "Mood updated to Great"

---

## Future Enhancements (Out of Scope)

- **Mood History**: Store all mood changes with timestamps
- **Mood Reasons**: Allow optional text note (e.g., "Blocked by API issue")
- **Mood Trends**: Show mood graph over time
- **Mood Notifications**: Notify team lead when someone selects "Overwhelmed"
- **Mood Privacy**: Allow team members to hide mood from others
