using backend.Models;

namespace backend.Services;

public interface ITeamMemberService
{
    Task<List<TeamMember>> GetAllAsync(bool includeGoals = false);
    Task<TeamMember> UpdateMoodAsync(int id, Mood mood);
}
