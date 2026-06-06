namespace WhatsForDinner.Api.Models.Dtos;

public record RecipeDto(
    int Id,
    string Name,
    string? Description,
    string? Ingredients,
    string? Preparation,
    int? CookTimeMinutes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
