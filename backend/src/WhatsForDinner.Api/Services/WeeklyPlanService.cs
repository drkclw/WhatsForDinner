using Microsoft.EntityFrameworkCore;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public class WeeklyPlanService : IWeeklyPlanService
{
    private readonly ApplicationDbContext _context;

    public WeeklyPlanService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WeeklyPlanDto?> GetWeeklyPlanAsync(int userId = 1)
    {
        var weeklyPlan = await _context.WeeklyPlans
            .Include(wp => wp.Items)
                .ThenInclude(item => item.Recipe)
            .FirstOrDefaultAsync(wp => wp.UserId == userId);

        if (weeklyPlan == null)
        {
            return null;
        }

        return MapToDto(weeklyPlan);
    }

    public async Task<WeeklyPlanItemDto?> AddRecipeToWeeklyPlanAsync(int recipeId, int userId = 1)
    {
        var weeklyPlan = await _context.WeeklyPlans
            .FirstOrDefaultAsync(wp => wp.UserId == userId);

        if (weeklyPlan == null)
        {
            return null;
        }

        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe == null || recipe.UserId != userId)
        {
            return null;
        }

        var item = new WeeklyPlanItem
        {
            WeeklyPlanId = weeklyPlan.Id,
            RecipeId = recipeId,
            AddedAt = DateTime.UtcNow
        };

        _context.WeeklyPlanItems.Add(item);
        weeklyPlan.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        // Reload the item with recipe
        await _context.Entry(item).Reference(i => i.Recipe).LoadAsync();

        return MapItemToDto(item);
    }

    public async Task<bool> RemoveFromWeeklyPlanAsync(int weeklyPlanItemId, int userId = 1)
    {
        var item = await _context.WeeklyPlanItems
            .Include(i => i.WeeklyPlan)
            .FirstOrDefaultAsync(i => i.Id == weeklyPlanItemId && i.WeeklyPlan.UserId == userId);

        if (item == null)
        {
            return false;
        }

        _context.WeeklyPlanItems.Remove(item);
        item.WeeklyPlan.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    private static WeeklyPlanDto MapToDto(WeeklyPlan weeklyPlan)
    {
        return new WeeklyPlanDto(
            weeklyPlan.Id,
            weeklyPlan.Items.Select(MapItemToDto).ToList(),
            weeklyPlan.CreatedAt,
            weeklyPlan.UpdatedAt
        );
    }

    private static WeeklyPlanItemDto MapItemToDto(WeeklyPlanItem item)
    {
        return new WeeklyPlanItemDto(
            item.Id,
            new RecipeDto(
                item.Recipe.Id,
                item.Recipe.Name,
                item.Recipe.Description,
                item.Recipe.Ingredients,
                item.Recipe.CookTimeMinutes,
                item.Recipe.CreatedAt,
                item.Recipe.UpdatedAt
            ),
            item.AddedAt
        );
    }
}
