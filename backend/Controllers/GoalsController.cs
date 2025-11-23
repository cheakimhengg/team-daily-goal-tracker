using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/goals")]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoalCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid request data", details = ModelState } });
        }

        var goal = await _goalService.CreateAsync(request.TeamMemberId, request.GoalText);

        return CreatedAtAction(nameof(Create), new { data = goal });
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleCompletion(int id)
    {
        var goal = await _goalService.ToggleCompletionAsync(id);

        return Ok(new { data = goal });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _goalService.DeleteAsync(id);

        return NoContent();
    }
}
