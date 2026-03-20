using System.ComponentModel.DataAnnotations;

namespace WhatsForDinner.Api.Models.Dtos;

public record RecipeUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    [StringLength(2000)]
    public string? Ingredients { get; init; }

    [Range(0, int.MaxValue)]
    public int? CookTimeMinutes { get; init; }
}
