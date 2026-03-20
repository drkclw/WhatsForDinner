using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Unit.Services;

public class WeeklyPlanServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        
        // Seed test data
        var user = new User { Id = 1, Name = "Test User", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var recipe1 = new Recipe { Id = 1, UserId = 1, Name = "Test Recipe 1", CookTimeMinutes = 30 };
        var recipe2 = new Recipe { Id = 2, UserId = 1, Name = "Test Recipe 2", CookTimeMinutes = 45 };
        context.Recipes.AddRange(recipe1, recipe2);

        var weeklyPlan = new WeeklyPlan { Id = 1, UserId = 1 };
        context.WeeklyPlans.Add(weeklyPlan);

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetWeeklyPlanAsync_ReturnsWeeklyPlan_WhenPlanExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);

        // Act
        var result = await service.GetWeeklyPlanAsync(userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWeeklyPlanAsync_ReturnsNull_WhenPlanDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);

        // Act
        var result = await service.GetWeeklyPlanAsync(userId: 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddRecipeToWeeklyPlanAsync_AddsRecipe_WhenRecipeExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);

        // Act
        var result = await service.AddRecipeToWeeklyPlanAsync(recipeId: 1, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Recipe.Id.Should().Be(1);
        result.Recipe.Name.Should().Be("Test Recipe 1");
        
        // Verify it was persisted
        var planItems = await context.WeeklyPlanItems.ToListAsync();
        planItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRecipeToWeeklyPlanAsync_ReturnsNull_WhenRecipeDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);

        // Act
        var result = await service.AddRecipeToWeeklyPlanAsync(recipeId: 999, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFromWeeklyPlanAsync_ReturnsTrue_WhenItemExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);
        
        // First add an item
        var addedItem = await service.AddRecipeToWeeklyPlanAsync(recipeId: 1, userId: 1);
        addedItem.Should().NotBeNull();

        // Act
        var result = await service.RemoveFromWeeklyPlanAsync(addedItem!.Id, userId: 1);

        // Assert
        result.Should().BeTrue();
        
        // Verify it was removed
        var planItems = await context.WeeklyPlanItems.ToListAsync();
        planItems.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveFromWeeklyPlanAsync_ReturnsFalse_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new WeeklyPlanService(context);

        // Act
        var result = await service.RemoveFromWeeklyPlanAsync(weeklyPlanItemId: 999, userId: 1);

        // Assert
        result.Should().BeFalse();
    }
}
