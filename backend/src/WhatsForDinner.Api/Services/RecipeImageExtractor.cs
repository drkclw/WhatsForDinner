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
        var endpoint = _configuration["OpenAI:Endpoint"];

        try
        {
            ChatClient chatClient;
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
                var client = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
                chatClient = client.GetChatClient(model);
            }
            else
            {
                var client = new OpenAIClient(new ApiKeyCredential(apiKey));
                chatClient = client.GetChatClient(model);
            }

            var base64Image = Convert.ToBase64String(imageData);
            var mediaType = contentType;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a recipe extraction assistant. Analyze the provided image and extract recipe information. " +
                    "Extract as much of the following as you can see in the image: name (exactly as it appears — do NOT infer or fabricate it), description, ingredients, and cook time. " +
                    "Set any field to null if it is not clearly visible in the image. " +
                    "Only return all nulls if the image contains no recipe information at all. " +
                    "Respond ONLY with valid JSON matching this schema: " +
                    "{\"name\": string|null, \"description\": string|null, \"ingredients\": string|null, \"cookTimeMinutes\": integer|null}"
                ),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Extract recipe information from this image. Use only text and information visible in the image — do not infer or fabricate the recipe name."),
                    ChatMessageContentPart.CreateImagePart(
                        new BinaryData(imageData),
                        mediaType,
                        ChatImageDetailLevel.High
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

            return BuildResult(extracted);
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

    internal static RecipeImageExtractResult BuildResult(ExtractedRecipe? extracted)
    {
        if (extracted == null)
        {
            return new RecipeImageExtractResult(
                Success: false,
                Message: "Could not extract recipe information from the image"
            );
        }

        // Treat negative cook time as unreadable
        var cookTime = extracted.CookTimeMinutes is >= 0 ? extracted.CookTimeMinutes : null;

        if (string.IsNullOrWhiteSpace(extracted.Name) &&
            string.IsNullOrWhiteSpace(extracted.Description) &&
            string.IsNullOrWhiteSpace(extracted.Ingredients) &&
            cookTime == null)
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
            CookTimeMinutes: cookTime,
            Message: "Recipe extracted successfully"
        );
    }

    internal record ExtractedRecipe
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? Ingredients { get; init; }
        public int? CookTimeMinutes { get; init; }
    }
}
