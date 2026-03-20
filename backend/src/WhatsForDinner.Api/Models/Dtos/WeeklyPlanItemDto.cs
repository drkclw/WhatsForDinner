namespace WhatsForDinner.Api.Models.Dtos;

public record WeeklyPlanItemDto(
    int Id,
    RecipeDto Recipe,
    DateTime AddedAt
);
