namespace WhatsForDinner.Api.Models;

public class WeeklyPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WeeklyPlanItem> Items { get; set; } = new List<WeeklyPlanItem>();
}
