using backend.Models;

namespace backend.Data.Repositories;

public interface IGoalRepository
{
    Task<Goal> InsertAsync(Goal goal);
    Task<Goal?> GetByIdAsync(int id);
    Task ToggleCompletionAsync(int id);
    Task<int> DeleteAsync(int id);
}
