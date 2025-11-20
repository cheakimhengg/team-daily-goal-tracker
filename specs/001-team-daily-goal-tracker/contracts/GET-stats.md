# GET /api/stats

**User Story**: US4 (View Stats) - Priority P2
**Purpose**: Retrieve aggregated statistics for today's goals and team moods

---

## Request

### HTTP Method
```
GET /api/stats
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `date` | string (ISO 8601) | No | Today | Date for statistics (format: `YYYY-MM-DD`) |

### Headers
```
Content-Type: application/json
```

### Request Example

```http
GET /api/stats HTTP/1.1
Host: localhost:5000
Content-Type: application/json
```

**With Custom Date**:
```http
GET /api/stats?date=2025-11-19 HTTP/1.1
```

---

## Response

### Success Response (200 OK)

```json
{
  "data": {
    "date": "2025-11-20",
    "totalGoals": 15,
    "completedGoals": 8,
    "completionRate": 53.33,
    "moodBreakdown": {
      "Great": 3,
      "Good": 5,
      "Okay": 2,
      "Struggling": 1,
      "Overwhelmed": 0
    },
    "teamSize": 11
  }
}
```

### Response Schema

#### StatsResponse

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `date` | string (ISO 8601 date) | No | Date for these statistics (e.g., `"2025-11-20"`) |
| `totalGoals` | integer | No | Total number of goals created on this date |
| `completedGoals` | integer | No | Number of completed goals created on this date |
| `completionRate` | number (float) | No | Percentage of goals completed (0-100, rounded to 2 decimals) |
| `moodBreakdown` | object | No | Count of team members in each mood state |
| `teamSize` | integer | No | Total number of team members (includes those with no mood set) |

#### MoodBreakdown Object

| Field | Type | Description |
|-------|------|-------------|
| `Great` | integer | Count of team members with mood "Great" |
| `Good` | integer | Count of team members with mood "Good" |
| `Okay` | integer | Count of team members with mood "Okay" |
| `Struggling` | integer | Count of team members with mood "Struggling" |
| `Overwhelmed` | integer | Count of team members with mood "Overwhelmed" |

**Note**: Team members with `currentMood = null` are **not included** in `moodBreakdown` counts but **are included** in `teamSize`.

---

## Error Responses

### 400 Bad Request - Invalid Date Format

**Scenario**: `date` parameter is not in valid ISO 8601 format

```json
{
  "error": {
    "code": "INVALID_DATE_FORMAT",
    "message": "Date must be in YYYY-MM-DD format (e.g., 2025-11-20)"
  }
}
```

### 500 Internal Server Error

**Scenario**: Database connection failure or unexpected server error

```json
{
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred while retrieving statistics"
  }
}
```

---

## Business Rules

1. **Date Filtering**: Goals are filtered by `CreatedAt` date (ignores time component)
2. **Completion Rate Calculation**:
   - If `totalGoals = 0`, then `completionRate = 0.0`
   - Otherwise: `completionRate = (completedGoals / totalGoals) * 100`
   - Rounded to 2 decimal places
3. **Mood Snapshot**: Uses **current** mood state (not historical moods)
4. **Null Moods**: Team members with `currentMood = null` are excluded from `moodBreakdown` but counted in `teamSize`
5. **Default Date**: If no `date` parameter provided, uses current UTC date

---

## Calculation Examples

### Example 1: Typical Day
- Total goals created today: 15
- Completed goals: 8
- Completion rate: `(8 / 15) * 100 = 53.33%`

### Example 2: No Goals
- Total goals: 0
- Completed goals: 0
- Completion rate: `0.0%` (avoid division by zero)

### Example 3: All Goals Completed
- Total goals: 10
- Completed goals: 10
- Completion rate: `(10 / 10) * 100 = 100.0%`

---

## Performance

- **Expected Response Time**: < 100ms (multiple aggregate queries)
- **Caching**: Can be cached for 1-5 minutes (data doesn't change frequently)
- **Query Optimization**: Uses indexed `Goals.CreatedAt` column

---

## Example Usage (Frontend)

### TypeScript/Vue 3

```typescript
// services/api.ts
export async function getStats(date?: string): Promise<Stats> {
  const url = date
    ? `http://localhost:5000/api/stats?date=${date}`
    : 'http://localhost:5000/api/stats';

  const response = await fetch(url, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' }
  });

  if (!response.ok) {
    throw new Error('Failed to fetch statistics');
  }

  const json = await response.json();
  return json.data;
}
```

### Component Usage

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getStats } from '@/services/api';
import type { Stats } from '@/types/Stats';

const stats = ref<Stats | null>(null);

onMounted(async () => {
  stats.value = await getStats(); // Fetch today's stats
});
</script>

<template>
  <div v-if="stats" class="stats shadow">
    <div class="stat">
      <div class="stat-title">Total Goals Today</div>
      <div class="stat-value">{{ stats.totalGoals }}</div>
    </div>

    <div class="stat">
      <div class="stat-title">Completed</div>
      <div class="stat-value text-success">{{ stats.completedGoals }}</div>
    </div>

    <div class="stat">
      <div class="stat-title">Completion Rate</div>
      <div class="stat-value">{{ stats.completionRate.toFixed(1) }}%</div>
    </div>
  </div>

  <div class="mt-4">
    <h3>Team Mood Breakdown</h3>
    <div v-for="(count, mood) in stats.moodBreakdown" :key="mood" class="badge">
      {{ mood }}: {{ count }}
    </div>
  </div>
</template>
```

---

## Backend Implementation Notes

### C# Controller

```csharp
[HttpGet]
public async Task<IActionResult> GetDailyStats([FromQuery] string? date = null)
{
    DateTime targetDate;

    if (string.IsNullOrEmpty(date))
    {
        targetDate = DateTime.UtcNow.Date;
    }
    else
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "INVALID_DATE_FORMAT",
                    message = "Date must be in YYYY-MM-DD format (e.g., 2025-11-20)"
                }
            });
        }
    }

    var stats = await _statsService.GetDailyStatsAsync(targetDate);
    return Ok(new { data = stats });
}
```

### Response DTO

```csharp
public class StatsResponse
{
    public string Date { get; set; } = string.Empty; // "2025-11-20"
    public int TotalGoals { get; set; }
    public int CompletedGoals { get; set; }
    public double CompletionRate { get; set; } // 0.0 - 100.0
    public Dictionary<string, int> MoodBreakdown { get; set; } = new();
    public int TeamSize { get; set; }
}
```

### Dapper Queries

```csharp
public async Task<StatsResponse> GetDailyStatsAsync(DateTime date)
{
    var dateString = date.ToString("yyyy-MM-dd");

    // Query 1: Total goals created on date
    var totalGoalsQuery = @"
        SELECT COUNT(*)
        FROM Goals
        WHERE DATE(CreatedAt) = @Date
    ";
    var totalGoals = await _connection.QuerySingleAsync<int>(totalGoalsQuery, new { Date = dateString });

    // Query 2: Completed goals created on date
    var completedGoalsQuery = @"
        SELECT COUNT(*)
        FROM Goals
        WHERE DATE(CreatedAt) = @Date AND IsCompleted = 1
    ";
    var completedGoals = await _connection.QuerySingleAsync<int>(completedGoalsQuery, new { Date = dateString });

    // Query 3: Mood breakdown (current moods, not historical)
    var moodBreakdownQuery = @"
        SELECT CurrentMood, COUNT(*) as Count
        FROM TeamMembers
        WHERE CurrentMood IS NOT NULL
        GROUP BY CurrentMood
    ";
    var moodCounts = await _connection.QueryAsync<(string Mood, int Count)>(moodBreakdownQuery);

    // Query 4: Total team size
    var teamSizeQuery = "SELECT COUNT(*) FROM TeamMembers";
    var teamSize = await _connection.QuerySingleAsync<int>(teamSizeQuery);

    // Calculate completion rate
    var completionRate = totalGoals > 0
        ? Math.Round((double)completedGoals / totalGoals * 100, 2)
        : 0.0;

    // Build mood breakdown dictionary (ensure all moods are present)
    var moodBreakdown = new Dictionary<string, int>
    {
        { "Great", 0 },
        { "Good", 0 },
        { "Okay", 0 },
        { "Struggling", 0 },
        { "Overwhelmed", 0 }
    };

    foreach (var (mood, count) in moodCounts)
    {
        moodBreakdown[mood] = count;
    }

    return new StatsResponse
    {
        Date = dateString,
        TotalGoals = totalGoals,
        CompletedGoals = completedGoals,
        CompletionRate = completionRate,
        MoodBreakdown = moodBreakdown,
        TeamSize = teamSize
    };
}
```

---

## Testing

### Test Cases

1. **TC-01**: GET without date parameter → Returns today's stats
2. **TC-02**: GET with valid date → Returns stats for that date
3. **TC-03**: Date with no goals → Returns `totalGoals: 0`, `completionRate: 0.0`
4. **TC-04**: All moods present in breakdown (even if count is 0)
5. **TC-05**: Team members with null mood excluded from `moodBreakdown`, included in `teamSize`
6. **TC-06**: Completion rate calculation accuracy (test with various ratios)
7. **TC-07**: Invalid date format → Returns 400

### Contract Test Example (xUnit)

```csharp
[Fact]
public async Task GetStats_WithoutDate_ReturnsTodayStats()
{
    // Arrange: Seed database with test data for today
    await SeedTestGoals(DateTime.UtcNow.Date, totalCount: 10, completedCount: 6);

    // Act
    var response = await _client.GetAsync("/api/stats");

    // Assert
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<StatsApiResponse>(json);

    Assert.Equal(10, result.Data.TotalGoals);
    Assert.Equal(6, result.Data.CompletedGoals);
    Assert.Equal(60.0, result.Data.CompletionRate);
}

[Fact]
public async Task GetStats_WithNoGoals_ReturnsZeroCompletionRate()
{
    // Arrange: Ensure no goals for today
    await ClearGoals();

    // Act
    var response = await _client.GetAsync("/api/stats");

    // Assert
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<StatsApiResponse>(json);

    Assert.Equal(0, result.Data.TotalGoals);
    Assert.Equal(0, result.Data.CompletedGoals);
    Assert.Equal(0.0, result.Data.CompletionRate);
}

[Fact]
public async Task GetStats_MoodBreakdown_IncludesAllMoods()
{
    // Act
    var response = await _client.GetAsync("/api/stats");

    // Assert
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<StatsApiResponse>(json);

    Assert.Contains("Great", result.Data.MoodBreakdown.Keys);
    Assert.Contains("Good", result.Data.MoodBreakdown.Keys);
    Assert.Contains("Okay", result.Data.MoodBreakdown.Keys);
    Assert.Contains("Struggling", result.Data.MoodBreakdown.Keys);
    Assert.Contains("Overwhelmed", result.Data.MoodBreakdown.Keys);
}
```

---

## UI/UX Considerations

### Stats Panel Layout

```
┌─────────────────────────────────────────────┐
│  Daily Statistics - November 20, 2025       │
├─────────────────────────────────────────────┤
│  📊 Total Goals: 15                         │
│  ✅ Completed: 8                            │
│  📈 Completion Rate: 53.33%                 │
├─────────────────────────────────────────────┤
│  Team Mood Breakdown (11 members):          │
│  😊 Great: 3   🙂 Good: 5   😐 Okay: 2      │
│  😟 Struggling: 1   😰 Overwhelmed: 0       │
└─────────────────────────────────────────────┘
```

### Visual Indicators
- **Completion Rate**:
  - 0-30% → Red (needs attention)
  - 31-70% → Yellow (on track)
  - 71-100% → Green (excellent)
- **Mood Colors**: Match mood button colors (green for Great, red for Overwhelmed)

---

## Caching Strategy (Optional Enhancement)

For production, consider caching stats for 1-5 minutes:

```csharp
[ResponseCache(Duration = 300)] // Cache for 5 minutes
public async Task<IActionResult> GetDailyStats([FromQuery] string? date = null)
{
    // ...
}
```

**Tradeoff**: Slightly stale data vs. reduced database load

---

## Future Enhancements (Out of Scope)

- **Historical Stats**: Add date range parameter (e.g., `?from=2025-11-01&to=2025-11-20`)
- **Team Comparison**: Compare current day to previous days/weeks
- **Goal Categories**: Break down stats by goal category
- **Individual Stats**: Add `/api/stats/team-members/{id}` for per-member stats
- **Export**: Add `?format=csv` to download stats as CSV
- **Real-time Updates**: WebSocket push for live stat updates
