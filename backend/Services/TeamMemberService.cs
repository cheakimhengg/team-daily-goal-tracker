using backend.Data.Repositories;
using backend.Exceptions;
using backend.Models;

namespace backend.Services;

public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamMemberRepository _teamMemberRepository;

    public TeamMemberService(ITeamMemberRepository teamMemberRepository)
    {
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<List<TeamMember>> GetAllAsync(bool includeGoals = false)
    {
        return await _teamMemberRepository.GetAllAsync(includeGoals);
    }

    public async Task<TeamMember> UpdateMoodAsync(int id, Mood mood)
    {
        var teamMember = await _teamMemberRepository.GetByIdAsync(id);
        if (teamMember == null)
        {
            throw new TeamMemberNotFoundException($"Team member with ID {id} does not exist");
        }

        var timestamp = DateTime.UtcNow;
        await _teamMemberRepository.UpdateMoodAsync(id, mood, timestamp);

        teamMember.CurrentMood = mood;
        teamMember.MoodUpdatedAt = timestamp;

        return teamMember;
    }
}
