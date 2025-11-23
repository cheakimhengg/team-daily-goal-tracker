using backend.Data.Repositories;
using backend.Exceptions;
using backend.Models;

namespace backend.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;

    public GoalService(IGoalRepository goalRepository, ITeamMemberRepository teamMemberRepository)
    {
        _goalRepository = goalRepository;
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<Goal> CreateAsync(int teamMemberId, string goalText)
    {
        // Validate team member exists
        var teamMember = await _teamMemberRepository.GetByIdAsync(teamMemberId);
        if (teamMember == null)
        {
            throw new TeamMemberNotFoundException($"Team member with ID {teamMemberId} does not exist");
        }

        // Create goal entity
        var goal = new Goal
        {
            TeamMemberId = teamMemberId,
            GoalText = goalText,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        // Insert and return
        return await _goalRepository.InsertAsync(goal);
    }

    public async Task<Goal> ToggleCompletionAsync(int id)
    {
        // Toggle completion
        await _goalRepository.ToggleCompletionAsync(id);

        // Fetch updated goal
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null)
        {
            throw new GoalNotFoundException($"Goal with ID {id} does not exist");
        }

        return goal;
    }

    public async Task DeleteAsync(int id)
    {
        var rowsAffected = await _goalRepository.DeleteAsync(id);
        if (rowsAffected == 0)
        {
            throw new GoalNotFoundException($"Goal with ID {id} does not exist");
        }
    }
}
