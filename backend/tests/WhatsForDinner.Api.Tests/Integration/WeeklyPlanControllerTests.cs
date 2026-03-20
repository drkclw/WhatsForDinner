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

public class WeeklyPlanControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeeklyPlanControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("TestDb_WeeklyPlan_" + Guid.NewGuid());
                });

                // Build the service provider and seed data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                
                // Seed test data
                var user = new User { Id = 1, Name = "Test User" };
                db.Users.Add(user);

                var recipe = new Recipe { Id = 1, UserId = 1, Name = "Test Recipe", CookTimeMinutes = 30 };
                db.Recipes.Add(recipe);

                var weeklyPlan = new WeeklyPlan { Id = 1, UserId = 1 };
                db.WeeklyPlans.Add(weeklyPlan);

                db.SaveChanges();
            });
        });
    }

    [Fact]
    public async Task GetWeeklyPlan_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/weekly-plan");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WeeklyPlanDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToWeeklyPlan_ReturnsCreated_WhenRecipeExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddToWeeklyPlanRequest { RecipeId = 1 };

        // Act
        var response = await client.PostAsJsonAsync("/api/weekly-plan/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<WeeklyPlanItemDto>();
        result.Should().NotBeNull();
        result!.Recipe.Id.Should().Be(1);
    }

    [Fact]
    public async Task AddToWeeklyPlan_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddToWeeklyPlanRequest { RecipeId = 999 };

        // Act
        var response = await client.PostAsJsonAsync("/api/weekly-plan/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveFromWeeklyPlan_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/weekly-plan/items/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
