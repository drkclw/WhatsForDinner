using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IWeeklyPlanService
{
    Task<WeeklyPlanDto?> GetWeeklyPlanAsync(int userId);
    Task<WeeklyPlanItemDto?> AddRecipeToWeeklyPlanAsync(int recipeId, int userId);
    Task<bool> RemoveFromWeeklyPlanAsync(int weeklyPlanItemId, int userId);
}
