using backend.Models;

namespace backend.Services;

public interface IGoalService
{
    Task<Goal> CreateAsync(int teamMemberId, string goalText);
    Task<Goal> ToggleCompletionAsync(int id);
    Task DeleteAsync(int id);
}
