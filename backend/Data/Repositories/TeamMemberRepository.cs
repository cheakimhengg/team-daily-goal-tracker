using System.Data;
using Dapper;
using backend.Models;

namespace backend.Data.Repositories;

public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TeamMemberRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<TeamMember>> GetAllAsync(bool includeGoals = false)
    {
        using var connection = _connectionFactory.CreateConnection();

        if (!includeGoals)
        {
            var query = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers ORDER BY Name";
            var teamMembers = await connection.QueryAsync<TeamMember>(query);
            return teamMembers.ToList();
        }

        // Multi-mapping query to include goals
        var queryWithGoals = @"
            SELECT
                tm.Id, tm.Name, tm.CurrentMood, tm.MoodUpdatedAt,
                g.Id, g.TeamMemberId, g.GoalText, g.CreatedAt, g.IsCompleted
            FROM TeamMembers tm
            LEFT JOIN Goals g ON tm.Id = g.TeamMemberId
            ORDER BY tm.Name, g.CreatedAt DESC
        ";

        var teamMemberDict = new Dictionary<int, TeamMember>();

        await connection.QueryAsync<TeamMember, Goal, TeamMember>(
            queryWithGoals,
            (teamMember, goal) =>
            {
                if (!teamMemberDict.TryGetValue(teamMember.Id, out var existingTeamMember))
                {
                    existingTeamMember = teamMember;
                    existingTeamMember.Goals = new List<Goal>();
                    teamMemberDict.Add(teamMember.Id, existingTeamMember);
                }

                if (goal != null)
                {
                    existingTeamMember.Goals.Add(goal);
                }

                return existingTeamMember;
            },
            splitOn: "Id"
        );

        return teamMemberDict.Values.ToList();
    }

    public async Task<TeamMember?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = "SELECT Id, Name, CurrentMood, MoodUpdatedAt FROM TeamMembers WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<TeamMember>(query, new { Id = id });
    }

    public async Task UpdateMoodAsync(int id, Mood mood, DateTime timestamp)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = @"
            UPDATE TeamMembers
            SET CurrentMood = @Mood, MoodUpdatedAt = @Timestamp
            WHERE Id = @Id
        ";

        await connection.ExecuteAsync(query, new
        {
            Id = id,
            Mood = mood.ToString(),
            Timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}
