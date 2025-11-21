namespace backend.Models;

public class TeamMember
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Mood? CurrentMood { get; set; }
    public DateTime? MoodUpdatedAt { get; set; }

    // Navigation property (populated via Dapper multi-mapping)
    public List<Goal> Goals { get; set; } = new();
}
