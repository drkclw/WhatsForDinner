using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Integration;

public class RecipeExtractionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RecipeExtractionTests(WebApplicationFactory<Program> factory)
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
                var dbName = "TestDb_Extraction_" + Guid.NewGuid();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                // Replace the image extractor with a fake
                services.AddScoped<IRecipeImageExtractor, FakeRecipeImageExtractor>();

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    private static byte[] CreateFakeJpeg() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    private static MultipartFormDataContent CreateMultiFileContent(int fileCount)
    {
        var content = new MultipartFormDataContent();
        for (int i = 0; i < fileCount; i++)
        {
            var fileContent = new ByteArrayContent(CreateFakeJpeg());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "files", $"recipe{i + 1}.jpg");
        }
        return content;
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsOk_WithSingleFile()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = CreateMultiFileContent(1);

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsOk_WithMultipleFiles()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = CreateMultiFileContent(3);

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsOk_WithFiveFiles()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = CreateMultiFileContent(5);

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsBadRequest_WhenNoFilesProvided()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsBadRequest_WhenMoreThanFiveFiles()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = CreateMultiFileContent(6);

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExtractFromImage_ReturnsBadRequest_WhenUnsupportedFileType()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateFakeJpeg());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", "recipe.pdf");

        var response = await client.PostAsync("/api/recipes/extract-from-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// A fake extractor that returns a successful result without calling OpenAI
    /// </summary>
    private class FakeRecipeImageExtractor : IRecipeImageExtractor
    {
        public Task<RecipeImageExtractResult> ExtractFromImagesAsync(List<(byte[] Data, string ContentType)> images)
        {
            return Task.FromResult(new RecipeImageExtractResult(
                Success: true,
                Name: "Test Recipe",
                Description: "A test recipe",
                Ingredients: "flour, eggs",
                Preparation: "Mix and bake.",
                CookTimeMinutes: 30,
                Message: "Recipe extracted successfully"
            ));
        }
    }
}
