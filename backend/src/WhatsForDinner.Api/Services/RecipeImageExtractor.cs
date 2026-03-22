using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public class RecipeImageExtractor : IRecipeImageExtractor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecipeImageExtractor> _logger;

    public RecipeImageExtractor(IConfiguration configuration, ILogger<RecipeImageExtractor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RecipeImageExtractResult> ExtractFromImageAsync(byte[] imageData, string contentType)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        try
        {
            var client = new OpenAIClient(apiKey);
            var chatClient = client.GetChatClient(model);

            var base64Image = Convert.ToBase64String(imageData);
            var mediaType = contentType;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a recipe extraction assistant. Analyze the provided image and extract recipe information. " +
                    "If the image contains a recipe, extract the name, description, ingredients, and cook time. " +
                    "If the image does not contain a recipe or is unreadable, indicate failure. " +
                    "Respond ONLY with valid JSON matching this schema: " +
                    "{\"name\": string|null, \"description\": string|null, \"ingredients\": string|null, \"cookTimeMinutes\": number|null}"
                ),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Extract recipe information from this image."),
                    ChatMessageContentPart.CreateImagePart(
                        new BinaryData(imageData),
                        mediaType,
                        ChatImageDetailLevel.Low
                    )
                )
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "recipe_extraction",
                    BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "name": { "type": ["string", "null"] },
                            "description": { "type": ["string", "null"] },
                            "ingredients": { "type": ["string", "null"] },
                            "cookTimeMinutes": { "type": ["integer", "null"] }
                        },
                        "required": ["name", "description", "ingredients", "cookTimeMinutes"],
                        "additionalProperties": false
                    }
                    """),
                    jsonSchemaIsStrict: true
                )
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var completion = await chatClient.CompleteChatAsync(messages, options, cts.Token);

            var responseContent = completion.Value.Content[0].Text;
            _logger.LogInformation("OpenAI extraction response received");

            var extracted = JsonSerializer.Deserialize<ExtractedRecipe>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (extracted == null || string.IsNullOrWhiteSpace(extracted.Name))
            {
                return new RecipeImageExtractResult(
                    Success: false,
                    Message: "Could not extract recipe information from the image"
                );
            }

            return new RecipeImageExtractResult(
                Success: true,
                Name: extracted.Name,
                Description: extracted.Description,
                Ingredients: extracted.Ingredients,
                CookTimeMinutes: extracted.CookTimeMinutes,
                Message: "Recipe extracted successfully"
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("OpenAI request timed out");
            throw new TimeoutException("AI service request timed out");
        }
        catch (ClientResultException ex) when (ex.Status == 401)
        {
            _logger.LogError(ex, "OpenAI authentication failed");
            throw new UnauthorizedAccessException("AI service authentication failed");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "OpenAI API error (status {Status})", ex.Status);
            throw new HttpRequestException("AI service error: " + ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response");
            return new RecipeImageExtractResult(
                Success: false,
                Message: "Failed to parse extracted recipe data"
            );
        }
    }

    private record ExtractedRecipe
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? Ingredients { get; init; }
        public int? CookTimeMinutes { get; init; }
    }
}
