using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeDto>> GetRecipesAsync(int userId);
    Task<RecipeDto?> GetRecipeByIdAsync(int id, int userId);
    Task<RecipeDto?> UpdateRecipeAsync(int id, RecipeUpdateRequest request, int userId);
    Task<RecipeDto> CreateRecipeAsync(RecipeCreateRequest request, int userId);
    Task<bool> DeleteRecipeAsync(int id, int userId);
}
