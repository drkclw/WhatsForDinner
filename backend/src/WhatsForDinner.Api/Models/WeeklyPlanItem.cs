namespace WhatsForDinner.Api.Models;

public class WeeklyPlanItem
{
    public int Id { get; set; }
    public int WeeklyPlanId { get; set; }
    public int RecipeId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public WeeklyPlan WeeklyPlan { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}
