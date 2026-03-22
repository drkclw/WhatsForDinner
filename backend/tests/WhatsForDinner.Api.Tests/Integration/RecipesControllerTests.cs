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
                var dbName = "TestDb_Recipes_" + Guid.NewGuid();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Build the service provider and ensure DB is created (seed data comes from entity configurations)
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
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

    // T015 - POST /api/recipes integration tests

    [Fact]
    public async Task CreateRecipe_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
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
        var client = _factory.CreateClient();
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
        var client = _factory.CreateClient();
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
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/recipes/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

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
        var client = _factory.CreateClient();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        // Act
        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
