using System.Net;
using System.Net.Http.Headers;
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

public class RecipesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = $"TestDb_Recipes_{Guid.NewGuid()}";

    public RecipesControllerTests(WebApplicationFactory<Program> factory)
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
                    new User
                    {
                        Id = 1,
                        GoogleId = "google-user-1",
                        Email = "user1@example.com",
                        DisplayName = "User One"
                    },
                    new User
                    {
                        Id = 2,
                        GoogleId = "google-user-2",
                        Email = "user2@example.com",
                        DisplayName = "User Two"
                    });

                db.Recipes.AddRange(
                    new Recipe { Id = 1, UserId = 1, Name = "U1 Pasta", Description = "u1", CookTimeMinutes = 20 },
                    new Recipe { Id = 2, UserId = 1, Name = "U1 Soup", Description = "u1", CookTimeMinutes = 30 },
                    new Recipe { Id = 3, UserId = 2, Name = "U2 Tacos", Description = "u2", CookTimeMinutes = 25 });

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
    public async Task GetRecipes_ReturnsOk_WithRecipeList()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/recipes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Name.StartsWith("U1"));
    }

    [Fact]
    public async Task GetRecipe_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

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
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/recipes/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRecipe_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
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
        var client = CreateAuthenticatedClient();
        var updateRequest = new RecipeUpdateRequest
        {
            Name = "Updated Recipe"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/recipes/999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // T015 - POST /api/recipes integration tests

    [Fact]
    public async Task CreateRecipe_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var createRequest = new RecipeCreateRequest
        {
            Name = "New Recipe",
            Description = "A delicious new recipe",
            Ingredients = "Flour\nSugar\nEggs",
            CookTimeMinutes = 45
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/recipes", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RecipeDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Recipe");
        result.Description.Should().Be("A delicious new recipe");
        result.Ingredients.Should().Be("Flour\nSugar\nEggs");
        result.CookTimeMinutes.Should().Be(45);
        result.Id.Should().BeGreaterThan(0);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRecipe_ReturnsBadRequest_WhenNameIsMissing()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var createRequest = new { Description = "No name provided" };

        // Act
        var response = await client.PostAsJsonAsync("/api/recipes", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRecipe_ReturnsBadRequest_WhenCookTimeIsNegative()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var createRequest = new RecipeCreateRequest
        {
            Name = "Bad Recipe",
            CookTimeMinutes = -5
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/recipes", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // T031 - DELETE /api/recipes/{id} integration tests

    [Fact]
    public async Task DeleteRecipe_ReturnsNoContent_WhenRecipeExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.DeleteAsync("/api/recipes/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.DeleteAsync("/api/recipes/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // T024 - POST /api/recipes/extract-from-image integration tests

    [Fact]
    public async Task ExtractFromImage_ReturnsBadRequest_WhenUnsupportedFormat()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        // Act
        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRecipe_ReturnsNotFound_ForDifferentUserRecipe()
    {
        var client = CreateAuthenticatedClient(userId: 2);

        var response = await client.GetAsync("/api/recipes/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRecipes_ReturnsOnlyCurrentUserRecipes_ForSecondUser()
    {
        var client = CreateAuthenticatedClient(userId: 2);

        var response = await client.GetAsync("/api/recipes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("U2 Tacos");
    }
}
