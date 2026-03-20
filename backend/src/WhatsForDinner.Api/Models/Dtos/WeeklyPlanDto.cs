namespace WhatsForDinner.Api.Models.Dtos;

public record WeeklyPlanDto(
    int Id,
    IReadOnlyList<WeeklyPlanItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
