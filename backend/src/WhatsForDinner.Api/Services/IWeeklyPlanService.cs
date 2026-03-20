using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public interface IWeeklyPlanService
{
    Task<WeeklyPlanDto?> GetWeeklyPlanAsync(int userId = 1);
    Task<WeeklyPlanItemDto?> AddRecipeToWeeklyPlanAsync(int recipeId, int userId = 1);
    Task<bool> RemoveFromWeeklyPlanAsync(int weeklyPlanItemId, int userId = 1);
}
