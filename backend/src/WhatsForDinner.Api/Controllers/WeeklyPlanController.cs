using Microsoft.AspNetCore.Mvc;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Controllers;

[ApiController]
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
        var weeklyPlan = await _weeklyPlanService.GetWeeklyPlanAsync();
        
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
        var item = await _weeklyPlanService.AddRecipeToWeeklyPlanAsync(request.RecipeId);
        
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
        var result = await _weeklyPlanService.RemoveFromWeeklyPlanAsync(id);
        
        if (!result)
        {
            return NotFound(new ErrorResponse("Weekly plan item not found"));
        }
        
        return NoContent();
    }
}
