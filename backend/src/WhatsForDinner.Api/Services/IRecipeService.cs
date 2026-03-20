using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeDto>> GetRecipesAsync(int userId = 1);
    Task<RecipeDto?> GetRecipeByIdAsync(int id, int userId = 1);
    Task<RecipeDto?> UpdateRecipeAsync(int id, RecipeUpdateRequest request, int userId = 1);
}
