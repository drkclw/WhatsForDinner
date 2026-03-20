namespace WhatsForDinner.Api.Models;

public class Recipe
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Ingredients { get; set; }
    public int? CookTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WeeklyPlanItem> WeeklyPlanItems { get; set; } = new List<WeeklyPlanItem>();
}
