using System.Data;
using Dapper;
using backend.Models;

namespace backend.Data.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GoalRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Goal> InsertAsync(Goal goal)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = @"
            INSERT INTO Goals (TeamMemberId, GoalText, CreatedAt, IsCompleted)
            VALUES (@TeamMemberId, @GoalText, @CreatedAt, @IsCompleted);
            SELECT last_insert_rowid();
        ";

        var id = await connection.ExecuteScalarAsync<int>(query, new
        {
            TeamMemberId = goal.TeamMemberId,
            GoalText = goal.GoalText,
            CreatedAt = goal.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            IsCompleted = goal.IsCompleted ? 1 : 0
        });

        goal.Id = id;
        return goal;
    }

    public async Task<Goal?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = "SELECT Id, TeamMemberId, GoalText, CreatedAt, IsCompleted FROM Goals WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<Goal>(query, new { Id = id });
    }

    public async Task ToggleCompletionAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = @"
            UPDATE Goals
            SET IsCompleted = CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END
            WHERE Id = @Id
        ";

        await connection.ExecuteAsync(query, new { Id = id });
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = "DELETE FROM Goals WHERE Id = @Id";

        return await connection.ExecuteAsync(query, new { Id = id });
    }
}
