namespace WhatsForDinner.Api.Models;

public class User
{
    public int Id { get; set; }
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public WeeklyPlan? WeeklyPlan { get; set; }
}
