namespace WhatsForDinner.Api.Models.Dtos;

public record RecipeImageExtractResult(
    bool Success,
    string? Name = null,
    string? Description = null,
    string? Ingredients = null,
    string? Preparation = null,
    int? CookTimeMinutes = null,
    string? Message = null
);
