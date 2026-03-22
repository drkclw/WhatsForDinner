using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IRecipeImageExtractor
{
    Task<RecipeImageExtractResult> ExtractFromImageAsync(byte[] imageData, string contentType);
}
