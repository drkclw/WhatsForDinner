using System.ComponentModel.DataAnnotations;

namespace WhatsForDinner.Api.Models.Dtos;

public record AddToWeeklyPlanRequest
{
    [Required]
    public required int RecipeId { get; init; }
}
