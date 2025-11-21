using Microsoft.AspNetCore.Mvc;
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
}
