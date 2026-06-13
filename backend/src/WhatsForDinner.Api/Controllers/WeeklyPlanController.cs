using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/weekly-plan")]
public class WeeklyPlanController : ControllerBase
{
    private readonly IWeeklyPlanService _weeklyPlanService;

    public WeeklyPlanController(IWeeklyPlanService weeklyPlanService)
    {
        _weeklyPlanService = weeklyPlanService;
    }

    /// <summary>
    /// Get the current user's weekly plan with recipes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WeeklyPlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeeklyPlanDto>> GetWeeklyPlan()
    {
        var userId = GetCurrentUserId();
        var weeklyPlan = await _weeklyPlanService.GetWeeklyPlanAsync(userId);
        
        if (weeklyPlan == null)
        {
            // For MVP, create a default plan if none exists
            return Ok(new WeeklyPlanDto(0, [], DateTime.UtcNow, DateTime.UtcNow));
        }
        
        return Ok(weeklyPlan);
    }

    /// <summary>
    /// Add a recipe to the weekly plan
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(WeeklyPlanItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeeklyPlanItemDto>> AddToWeeklyPlan([FromBody] AddToWeeklyPlanRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _weeklyPlanService.AddRecipeToWeeklyPlanAsync(request.RecipeId, userId);
        
        if (item == null)
        {
            return NotFound(new ErrorResponse("Recipe not found"));
        }
        
        return CreatedAtAction(nameof(GetWeeklyPlan), item);
    }

    /// <summary>
    /// Remove a recipe from the weekly plan
    /// </summary>
    [HttpDelete("items/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWeeklyPlan(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _weeklyPlanService.RemoveFromWeeklyPlanAsync(id, userId);
        
        if (!result)
        {
            return NotFound(new ErrorResponse("Weekly plan item not found"));
        }
        
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }

        return userId;
    }
}
