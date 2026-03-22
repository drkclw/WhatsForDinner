using Microsoft.EntityFrameworkCore;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _context;

    public RecipeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecipeDto>> GetRecipesAsync(int userId = 1)
    {
        var recipes = await _context.Recipes
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Name)
            .Select(r => new RecipeDto(
                r.Id,
                r.Name,
                r.Description,
                r.Ingredients,
                r.CookTimeMinutes,
                r.CreatedAt,
                r.UpdatedAt
            ))
            .ToListAsync();

        return recipes;
    }

    public async Task<RecipeDto?> GetRecipeByIdAsync(int id, int userId = 1)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recipe == null)
        {
            return null;
        }

        return new RecipeDto(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Ingredients,
            recipe.CookTimeMinutes,
            recipe.CreatedAt,
            recipe.UpdatedAt
        );
    }

    public async Task<RecipeDto?> UpdateRecipeAsync(int id, RecipeUpdateRequest request, int userId = 1)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recipe == null)
        {
            return null;
        }

        recipe.Name = request.Name;
        recipe.Description = request.Description;
        recipe.Ingredients = request.Ingredients;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RecipeDto(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Ingredients,
            recipe.CookTimeMinutes,
            recipe.CreatedAt,
            recipe.UpdatedAt
        );
    }

    public async Task<RecipeDto> CreateRecipeAsync(RecipeCreateRequest request, int userId = 1)
    {
        var recipe = new Recipe
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Ingredients = request.Ingredients,
            CookTimeMinutes = request.CookTimeMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        return new RecipeDto(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Ingredients,
            recipe.CookTimeMinutes,
            recipe.CreatedAt,
            recipe.UpdatedAt
        );
    }

    public async Task<bool> DeleteRecipeAsync(int id, int userId = 1)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recipe == null)
        {
            return false;
        }

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();

        return true;
    }
}
