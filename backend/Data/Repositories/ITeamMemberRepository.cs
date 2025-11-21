using backend.Models;

namespace backend.Data.Repositories;

public interface ITeamMemberRepository
{
    Task<List<TeamMember>> GetAllAsync(bool includeGoals = false);
    Task<TeamMember?> GetByIdAsync(int id);
    Task UpdateMoodAsync(int id, Mood mood, DateTime timestamp);
}
