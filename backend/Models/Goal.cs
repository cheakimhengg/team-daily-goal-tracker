namespace backend.Models;

public class Goal
{
    public int Id { get; set; }
    public int TeamMemberId { get; set; }
    public string GoalText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }

    // Navigation property (optional)
    public TeamMember? TeamMember { get; set; }
}
