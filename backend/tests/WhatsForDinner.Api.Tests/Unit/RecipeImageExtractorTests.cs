using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Unit;

public class RecipeImageExtractorTests
{
    private static RecipeImageExtractor CreateExtractor(string? apiKey = null, string model = "gpt-4o-mini", int timeoutSeconds = 90)
    {
        var configData = new Dictionary<string, string?>
        {
            ["OpenAI:ApiKey"] = apiKey ?? "",
            ["OpenAI:Model"] = model,
            ["OpenAI:TimeoutSeconds"] = timeoutSeconds.ToString()
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var logger = LoggerFactory.Create(b => { }).CreateLogger<RecipeImageExtractor>();
        return new RecipeImageExtractor(configuration, logger);
    }

    [Fact]
    public async Task ExtractFromImagesAsync_ThrowsInvalidOperationException_WhenApiKeyMissing()
    {
        // Arrange
        var extractor = CreateExtractor(apiKey: "");
        var images = new List<(byte[] Data, string ContentType)>
        {
            (new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "image/jpeg")
        };

        // Act
        Func<Task> act = () => extractor.ExtractFromImagesAsync(images);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not configured*");
    }

    [Fact]
    public async Task ExtractFromImagesAsync_ThrowsInvalidOperationException_WhenApiKeyIsWhitespace()
    {
        // Arrange
        var extractor = CreateExtractor(apiKey: "   ");
        var images = new List<(byte[] Data, string ContentType)>
        {
            (new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "image/jpeg")
        };

        // Act
        Func<Task> act = () => extractor.ExtractFromImagesAsync(images);

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

    [Fact]
    public void BuildResult_ReturnsSuccess_WhenOnlyPreparationIsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe { Preparation = "Preheat oven to 350F." };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Preparation.Should().Be("Preheat oven to 350F.");
    }

    [Fact]
    public void BuildResult_ReturnsPreparation_WhenAllFieldsPresent()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe
        {
            Name = "Pasta",
            Description = "Italian classic",
            Ingredients = "pasta, sauce",
            Preparation = "Boil pasta. Add sauce.",
            CookTimeMinutes = 20
        };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Name.Should().Be("Pasta");
        result.Preparation.Should().Be("Boil pasta. Add sauce.");
        result.CookTimeMinutes.Should().Be(20);
    }

    [Fact]
    public void BuildResult_ReturnsNullPreparation_WhenPreparationNotInImage()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe
        {
            Name = "Salad",
            Ingredients = "lettuce, tomato"
        };

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeTrue();
        result.Preparation.Should().BeNull();
    }

    [Fact]
    public void BuildResult_ReturnsFailure_WhenAllFieldsIncludingPreparationAreNull()
    {
        var extracted = new RecipeImageExtractor.ExtractedRecipe();

        var result = RecipeImageExtractor.BuildResult(extracted);

        result.Success.Should().BeFalse();
        result.Preparation.Should().BeNull();
    }
}
