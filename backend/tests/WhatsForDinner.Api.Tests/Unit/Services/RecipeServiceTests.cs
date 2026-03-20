using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Unit.Services;

public class RecipeServiceTests
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

        var recipe1 = new Recipe 
        { 
            Id = 1, 
            UserId = 1, 
            Name = "Spaghetti", 
            Description = "Italian pasta",
            CookTimeMinutes = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var recipe2 = new Recipe 
        { 
            Id = 2, 
            UserId = 1, 
            Name = "Salad", 
            Description = "Fresh greens",
            CookTimeMinutes = 15,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Recipes.AddRange(recipe1, recipe2);

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetRecipesAsync_ReturnsAllRecipesForUser()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new RecipeService(context);

        // Act
        var result = await service.GetRecipesAsync(userId: 1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Salad"); // Should be ordered by name
        result.Should().Contain(r => r.Name == "Spaghetti");
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ReturnsRecipe_WhenRecipeExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new RecipeService(context);

        // Act
        var result = await service.GetRecipeByIdAsync(id: 1, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Spaghetti");
        result.Description.Should().Be("Italian pasta");
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ReturnsNull_WhenRecipeDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new RecipeService(context);

        // Act
        var result = await service.GetRecipeByIdAsync(id: 999, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRecipeAsync_UpdatesRecipe_WhenRecipeExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new RecipeService(context);
        var updateRequest = new RecipeUpdateRequest
        {
            Name = "Updated Spaghetti",
            Description = "Updated description",
            Ingredients = "Pasta, sauce",
            CookTimeMinutes = 45
        };

        // Act
        var result = await service.UpdateRecipeAsync(id: 1, updateRequest, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Spaghetti");
        result.Description.Should().Be("Updated description");
        result.Ingredients.Should().Be("Pasta, sauce");
        result.CookTimeMinutes.Should().Be(45);
        
        // Verify it was persisted
        var updatedRecipe = await context.Recipes.FindAsync(1);
        updatedRecipe!.Name.Should().Be("Updated Spaghetti");
    }

    [Fact]
    public async Task UpdateRecipeAsync_ReturnsNull_WhenRecipeDoesNotExist()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new RecipeService(context);
        var updateRequest = new RecipeUpdateRequest
        {
            Name = "Updated Recipe"
        };

        // Act
        var result = await service.UpdateRecipeAsync(id: 999, updateRequest, userId: 1);

        // Assert
        result.Should().BeNull();
    }
}
