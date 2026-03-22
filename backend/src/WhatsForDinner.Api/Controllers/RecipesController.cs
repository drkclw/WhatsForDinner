using Microsoft.AspNetCore.Mvc;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly IRecipeImageExtractor _imageExtractor;

    public RecipesController(IRecipeService recipeService, IRecipeImageExtractor imageExtractor)
    {
        _recipeService = recipeService;
        _imageExtractor = imageExtractor;
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
    /// Create a new recipe
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipeDto>> CreateRecipe([FromBody] RecipeCreateRequest request)
    {
        var recipe = await _recipeService.CreateRecipeAsync(request);
        return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
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

    /// <summary>
    /// Delete a recipe
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var deleted = await _recipeService.DeleteRecipeAsync(id);
        
        if (!deleted)
        {
            return NotFound(new ErrorResponse("Recipe not found"));
        }
        
        return NoContent();
    }

    /// <summary>
    /// Extract recipe data from an uploaded image
    /// </summary>
    [HttpPost("extract-from-image")]
    [RequestSizeLimit(10_485_760)]
    [ProducesResponseType(typeof(RecipeImageExtractResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<RecipeImageExtractResult>> ExtractFromImage([FromForm] IFormFile file)
    {
        // Validate content type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new ErrorResponse("Unsupported file format. Please upload a JPEG, PNG, or WebP image."));
        }

        // Validate magic bytes
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var imageData = stream.ToArray();

        if (!ValidateMagicBytes(imageData, file.ContentType))
        {
            return BadRequest(new ErrorResponse("File content does not match the expected image format."));
        }

        try
        {
            var result = await _imageExtractor.ExtractFromImageAsync(imageData, file.ContentType);

            if (!result.Success)
            {
                return UnprocessableEntity(new ErrorResponse(result.Message ?? "Could not extract recipe from image"));
            }

            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse("AI service is not configured. Please set the OpenAI API key."));
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new ErrorResponse("AI service authentication failed. Please check the API key."));
        }
        catch (TimeoutException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new ErrorResponse("AI service request timed out. Please try again."));
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new ErrorResponse($"AI service error: {ex.Message}"));
        }
    }

    private static bool ValidateMagicBytes(byte[] data, string contentType)
    {
        if (data.Length < 4) return false;

        return contentType.ToLower() switch
        {
            "image/jpeg" => data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            "image/png" => data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
            "image/webp" => data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50,
            _ => false
        };
    }
}
