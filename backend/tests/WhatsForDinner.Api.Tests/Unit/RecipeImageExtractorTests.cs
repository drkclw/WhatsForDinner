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

    [Fact]
    public void BuildResult_ReturnsFailure_WhenExtractedIsNull()
    {
        var result = RecipeImageExtractor.BuildResult(null);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Could not extract");
    }

    [Fact]
    public void BuildResult_ReturnsFailure_WhenAllFieldsAreNull()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe();

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public void BuildResult_ReturnsSuccess_WhenOnlyNameIsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { Name = "Pasta" };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Name.Should().Be("Pasta");
    }

    [Fact]
    public void BuildResult_ReturnsSuccess_WhenOnlyIngredientsIsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { Ingredients = "flour, eggs" };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Ingredients.Should().Be("flour, eggs");
    }

    [Fact]
    public void BuildResult_ReturnsSuccess_WhenOnlyDescriptionIsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { Description = "A tasty dish" };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Description.Should().Be("A tasty dish");
    }

    [Fact]
    public void BuildResult_ReturnsSuccess_WhenOnlyCookTimeIsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { CookTimeMinutes = 30 };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.CookTimeMinutes.Should().Be(30);
    }

    [Fact]
    public void BuildResult_TreatsNegativeCookTime_AsNull()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { CookTimeMinutes = -5 };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeFalse();
        result.CookTimeMinutes.Should().BeNull();
    }

    [Fact]
    public void BuildResult_TreatsNegativeCookTime_AsNullWhenOtherFieldsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { Name = "Soup", CookTimeMinutes = -1 };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.CookTimeMinutes.Should().BeNull();
    }
}
