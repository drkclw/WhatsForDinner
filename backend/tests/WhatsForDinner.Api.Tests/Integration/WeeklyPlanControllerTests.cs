using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
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
    private readonly string _dbName = $"TestDb_WeeklyPlan_{Guid.NewGuid()}";

    public WeeklyPlanControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                               d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                
                db.Users.AddRange(
                    new User { Id = 1, GoogleId = "google-user-1", Email = "user1@example.com", DisplayName = "User One" },
                    new User { Id = 2, GoogleId = "google-user-2", Email = "user2@example.com", DisplayName = "User Two" });

                db.Recipes.AddRange(
                    new Recipe { Id = 1, UserId = 1, Name = "Test Recipe User1", CookTimeMinutes = 30 },
                    new Recipe { Id = 2, UserId = 2, Name = "Test Recipe User2", CookTimeMinutes = 45 });

                db.WeeklyPlans.AddRange(
                    new WeeklyPlan { Id = 1, UserId = 1 },
                    new WeeklyPlan { Id = 2, UserId = 2 });

                db.SaveChanges();
            });
        });
    }

    private HttpClient CreateAuthenticatedClient(int userId = 1)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, userId.ToString());
        return client;
    }

    [Fact]
    public async Task GetWeeklyPlan_ReturnsOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

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
        var client = CreateAuthenticatedClient();
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
        var client = CreateAuthenticatedClient();
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
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.DeleteAsync("/api/weekly-plan/items/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddToWeeklyPlan_ReturnsNotFound_WhenRecipeBelongsToDifferentUser()
    {
        var client = CreateAuthenticatedClient(userId: 1);
        var request = new AddToWeeklyPlanRequest { RecipeId = 2 };

        var response = await client.PostAsJsonAsync("/api/weekly-plan/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
