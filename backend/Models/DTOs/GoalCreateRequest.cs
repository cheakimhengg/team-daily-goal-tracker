using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs;

public class GoalCreateRequest
{
    [Required(ErrorMessage = "TeamMemberId is required")]
    public int TeamMemberId { get; set; }

    [Required(ErrorMessage = "GoalText is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "GoalText must be between 1 and 500 characters")]
    public string GoalText { get; set; } = string.Empty;
}
