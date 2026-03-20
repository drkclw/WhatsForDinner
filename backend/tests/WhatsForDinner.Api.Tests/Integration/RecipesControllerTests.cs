using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Tests.Integration;

public class RecipesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RecipesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all DbContext registrations
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                               d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_Recipes_" + Guid.NewGuid());
                });

                // Build the service provider and seed data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                
                // Seed test data
                var user = new User { Id = 1, Name = "Test User" };
                db.Users.Add(user);

                var recipe1 = new Recipe { Id = 1, UserId = 1, Name = "Spaghetti", CookTimeMinutes = 30 };
                var recipe2 = new Recipe { Id = 2, UserId = 1, Name = "Salad", CookTimeMinutes = 15 };
                db.Recipes.AddRange(recipe1, recipe2);

                db.SaveChanges();
            });
        });
    }

    [Fact]
    public async Task GetRecipes_ReturnsOk_WithRecipeList()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/recipes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetRecipe_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/recipes/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RecipeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/recipes/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRecipe_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var updateRequest = new RecipeUpdateRequest
        {
            Name = "Updated Recipe",
            Description = "New description"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/recipes/1", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RecipeDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Recipe");
    }

    [Fact]
    public async Task UpdateRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var updateRequest = new RecipeUpdateRequest
        {
            Name = "Updated Recipe"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/recipes/999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
