using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Unit;

public class RecipeImageExtractorTests
{
    private static RecipeImageExtractor CreateExtractor(string? apiKey = null, string model = "gpt-4o-mini")
    {
        var configData = new Dictionary<string, string?>
        {
            ["OpenAI:ApiKey"] = apiKey ?? "",
            ["OpenAI:Model"] = model
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var logger = LoggerFactory.Create(b => { }).CreateLogger<RecipeImageExtractor>();
        return new RecipeImageExtractor(configuration, logger);
    }

    [Fact]
    public async Task ExtractFromImageAsync_ThrowsInvalidOperationException_WhenApiKeyMissing()
    {
        // Arrange
        var extractor = CreateExtractor(apiKey: "");
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Fake JPEG header

        // Act
        Func<Task> act = () => extractor.ExtractFromImageAsync(imageData, "image/jpeg");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not configured*");
    }

    [Fact]
    public async Task ExtractFromImageAsync_ThrowsInvalidOperationException_WhenApiKeyIsWhitespace()
    {
        // Arrange
        var extractor = CreateExtractor(apiKey: "   ");
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        // Act
        Func<Task> act = () => extractor.ExtractFromImageAsync(imageData, "image/jpeg");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not configured*");
    }
}
