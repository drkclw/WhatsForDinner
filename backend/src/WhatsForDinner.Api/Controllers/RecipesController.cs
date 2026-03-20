using Microsoft.AspNetCore.Mvc;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    /// <summary>
    /// Get all recipes for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecipeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecipeDto>>> GetRecipes()
    {
        var recipes = await _recipeService.GetRecipesAsync();
        return Ok(recipes);
    }

    /// <summary>
    /// Get a specific recipe by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDto>> GetRecipe(int id)
    {
        var recipe = await _recipeService.GetRecipeByIdAsync(id);
        
        if (recipe == null)
        {
            return NotFound(new ErrorResponse("Recipe not found"));
        }
        
        return Ok(recipe);
    }

    /// <summary>
    /// Update a recipe
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipeDto>> UpdateRecipe(int id, [FromBody] RecipeUpdateRequest request)
    {
        var recipe = await _recipeService.UpdateRecipeAsync(id, request);
        
        if (recipe == null)
        {
            return NotFound(new ErrorResponse("Recipe not found"));
        }
        
        return Ok(recipe);
    }
}
