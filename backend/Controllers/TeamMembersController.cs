using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/team-members")]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;

    public TeamMembersController(ITeamMemberService teamMemberService)
    {
        _teamMemberService = teamMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeGoals = false)
    {
        var teamMembers = await _teamMemberService.GetAllAsync(includeGoals);
        return Ok(new { data = teamMembers });
    }

    [HttpPut("{id}/mood")]
    public async Task<IActionResult> UpdateMood(int id, [FromBody] MoodUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid request data", details = ModelState } });
        }

        var teamMember = await _teamMemberService.UpdateMoodAsync(id, request.Mood);

        return Ok(new { data = teamMember });
    }
}
