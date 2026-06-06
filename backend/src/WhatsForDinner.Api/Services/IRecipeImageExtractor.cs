using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IRecipeImageExtractor
{
    Task<RecipeImageExtractResult> ExtractFromImagesAsync(List<(byte[] Data, string ContentType)> images);
}
