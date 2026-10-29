using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Dishwise.Application.Dto;
using Dishwise.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Dishwise.Application.Services;

public class OpenAiService : IOpenAiService
{
    private readonly ChatClient? _chatClient;
    private readonly ILogger<OpenAiService> _logger;
    private readonly IConfiguration _config;

    public OpenAiService(
        IConfiguration config,
        ILogger<OpenAiService> logger)
    {
        _config = config;
        _logger = logger;

        // A missing key only fails when OpenAI is actually called, so imports that
        // don't need the AI fallback (e.g. pages with JSON-LD data) still work.
        var apiKey = _config["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var client = new OpenAIClient(apiKey);
            _chatClient = client.GetChatClient(_config["OpenAI:Model"] ?? "gpt-4o-mini");
        }
    }

    private ChatClient RequireChatClient() =>
        _chatClient ?? throw new InvalidOperationException("OpenAI ApiKey is not configured.");

    public async Task<RecipesDto> ParseRecipeAsync(
        string rawText,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("""
                You are a professional recipe parser. Extract recipe information and return ONLY valid JSON.
                DO NOT include markdown, code blocks, or explanations - just JSON.
                ENSURE the response is ALWAYS complete with closing braces.

                Return JSON with this exact structure:
                {
                  "title": "recipe title",
                  "mainCategory": "Breakfast|Lunch|Dinner|Dessert|Beverage|Baking",
                  "subCategory": "specific type (e.g. Chicken, Beef, Vegetarian, Cake, Soup)",
                  "description": "brief description",
                  "prepTimeMinutes": 10,
                  "cookTimeMinutes": 20,
                  "ingredients": [{"name": "ingredient", "quantity": 1, "unit": "cup"}],
                  "instructions": ["step 1", "step 2"],
                  "nutrition": {"calories": 250, "protein": 10, "carbohydrates": 30, "fat": 8, "fiber": 2, "sugar": 5, "sodium": 300, "potassium": 200, "calcium": 150, "iron": 2, "vitamin_A": 500, "vitamin_C": 10, "vitamin_D": 1, "cholesterol": 50}
                }

                CRITICAL REQUIREMENTS:
                1. MAIN CATEGORY MUST be one of: Breakfast, Lunch, Dinner, Dessert, Beverage, Baking (never use N/A, None, Unknown)
                2. SUB CATEGORY MUST be descriptive and never empty (e.g. Chicken, Beef, Vegetarian, Cake, Pasta - never N/A, None, Unknown)
                3. Extract ALL ingredients with name, quantity, unit
                4. List ALL instruction steps in order
                5. Infer nutrition per serving if not provided
                6. CRITICAL: Always end JSON with }
                7. CRITICAL: Return ONLY JSON, nothing else
                8. DO NOT include imageUrls - omit that field entirely
            """),
            ChatMessage.CreateUserMessage(rawText)
        };

        _logger.LogInformation("Sending recipe parsing request to OpenAI.");

        try
        {
            var response = await RequireChatClient().CompleteChatAsync(
                messages: messages,
                cancellationToken: cancellationToken);

            var content = response.Value.Content[0].Text;

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("OpenAI returned empty response.");

            _logger.LogInformation("OpenAI recipe parsing completed.");

            return MapOpenAiResponseToRecipeDto(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing recipe with OpenAI.");
            throw;
        }
    }

    public async Task<RecipesDto> ParseRecipeFromHtmlAsync(
        string html,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("""
                You are a professional recipe parser. Extract recipe information from HTML content.
                Return ONLY valid JSON with NO markdown, code blocks, or explanations.
                
                Return JSON with this exact structure:
                {
                  "title": "recipe title",
                  "mainCategory": "Breakfast|Lunch|Dinner|Dessert|Beverage|Baking",
                  "subCategory": "specific type",
                  "description": "brief description",
                  "prepTimeMinutes": 10,
                  "cookTimeMinutes": 20,
                  "ingredients": [{"name": "ingredient", "quantity": 1, "unit": "cup"}],
                  "instructions": ["step 1", "step 2"],
                  "nutrition": {"calories": 250, "protein": 10, "carbohydrates": 30, "fat": 8, "fiber": 2, "sugar": 5, "sodium": 300, "potassium": 200, "calcium": 150, "iron": 2, "vitamin_A": 500, "vitamin_C": 10, "vitamin_D": 1, "cholesterol": 50}
                }
                
                CRITICAL REQUIREMENTS:
                1. MAIN CATEGORY MUST be one of: Breakfast, Lunch, Dinner, Dessert, Beverage, Baking (never use N/A, None, Unknown)
                2. SUB CATEGORY MUST be descriptive and never empty (e.g. Chicken, Beef, Vegetarian, Cake, Pasta - never N/A, None, Unknown)
                3. Extract all ingredients with name, quantity, unit
                4. List all instruction steps in order
                5. Extract or estimate nutrition per serving
                6. Return ONLY valid JSON - no markdown, no code blocks, no explanations
                7. ALWAYS end with }
                8. DO NOT include imageUrls - omit that field entirely
                9. If the page is a recipe LIST or COLLECTION (not a single recipe), return title as "COLLECTION_PAGE" so the caller can detect it
            """),
            ChatMessage.CreateUserMessage($"Extract recipe information from this HTML:\n\n{html}")
        };

        _logger.LogInformation("Sending HTML directly to OpenAI for recipe parsing");

        try
        {
            var response = await RequireChatClient().CompleteChatAsync(
                messages: messages,
                cancellationToken: cancellationToken);

            var content = response.Value.Content[0].Text;

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("OpenAI returned empty response.");

            _logger.LogInformation("OpenAI HTML recipe parsing completed");

            return MapOpenAiResponseToRecipeDto(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing recipe HTML with OpenAI");
            throw;
        }
    }

    private static RecipesDto MapOpenAiResponseToRecipeDto(string json)
    {
        try
        {
            // Step 1: Clean the response - remove markdown code blocks if present
            var cleanedJson = json.Trim();
            
            // Remove markdown code blocks
            if (cleanedJson.StartsWith("```json"))
                cleanedJson = cleanedJson.Substring(7);
            if (cleanedJson.StartsWith("```"))
                cleanedJson = cleanedJson.Substring(3);
            if (cleanedJson.EndsWith("```"))
                cleanedJson = cleanedJson.Substring(0, cleanedJson.Length - 3);
            
            cleanedJson = cleanedJson.Trim();

            // Step 2: Remove any comments or invalid characters that might exist
            // Remove single-line comments (// comment)
            cleanedJson = System.Text.RegularExpressions.Regex.Replace(cleanedJson, @"//[^\n]*", "");
            
            // Step 3: Handle truncated JSON responses
            // If JSON ends with a quoted string that looks incomplete, try to fix it
            if (!cleanedJson.EndsWith("}"))
            {
                // Try to find the last complete object or array and close it
                int braceCount = 0;
                int bracketCount = 0;
                int lastValidPos = -1;

                for (int i = 0; i < cleanedJson.Length; i++)
                {
                    char c = cleanedJson[i];
                    
                    if (c == '{') braceCount++;
                    else if (c == '}') { braceCount--; if (braceCount == 0) lastValidPos = i; }
                    else if (c == '[') bracketCount++;
                    else if (c == ']') { bracketCount--; if (braceCount == 1 && bracketCount == 0) lastValidPos = i; }
                }

                // If we found a valid position before the truncation, use that
                if (lastValidPos > 0)
                {
                    cleanedJson = cleanedJson.Substring(0, lastValidPos + 1);
                    // Close any remaining open braces
                    while (braceCount > 0)
                    {
                        cleanedJson += "}";
                        braceCount--;
                    }
                }
                else
                {
                    // Response is too truncated to recover
                    throw new InvalidOperationException(
                        "Response is not valid JSON and cannot be auto-repaired - response appears truncated/incomplete. " +
                        "This may indicate OpenAI response was cut off. Try with a shorter recipe or simpler text.");
                }
            }

            // Remove any text after the final closing brace
            int lastBrace = cleanedJson.LastIndexOf("}");
            if (lastBrace > 0 && lastBrace < cleanedJson.Length - 1)
            {
                cleanedJson = cleanedJson.Substring(0, lastBrace + 1);
            }

            // Log raw response for debugging
            System.Diagnostics.Debug.WriteLine($"Cleaned JSON (first 300 chars): {cleanedJson.Substring(0, Math.Min(300, cleanedJson.Length))}");

            // Step 4: Deserialize the OpenAI response to an intermediate object
            var parsed = JsonSerializer.Deserialize<OpenAiRecipeResponse>(
                cleanedJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                }) ?? throw new InvalidOperationException("Deserialization resulted in null");

            // Validate minimum required fields
            if (string.IsNullOrWhiteSpace(parsed.Title))
                throw new InvalidOperationException("Recipe title is missing from OpenAI response");

            if (parsed.Title.Equals("COLLECTION_PAGE", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The URL appears to be a recipe collection or listing page, not a single recipe. Please provide a direct link to a single recipe.");

            // Log the parsed data for debugging
            System.Diagnostics.Debug.WriteLine($"OpenAI Response - Title: {parsed.Title}, Ingredients Count: {parsed.Ingredients?.Count ?? 0}, Instructions Count: {parsed.Instructions?.Count ?? 0}");

            // Map to RecipesDto
            var recipesDto = new RecipesDto
            {
                Title = parsed.Title ?? "Untitled Recipe",
                Description = parsed.Description ?? "",
                PrepTimeMinutes = parsed.PrepTimeMinutes > 0 ? parsed.PrepTimeMinutes : 0,
                CookTimeMinutes = parsed.CookTimeMinutes > 0 ? parsed.CookTimeMinutes : 0,
                MainCategoryName = parsed.MainCategory ?? "Dinner",
                SubCategoryName = parsed.SubCategory ?? "Main Course",
                Ingredients = ParseIngredients(parsed.Ingredients),
                Instructions = ParseInstructions(parsed.Instructions),
                Images = ParseImages(parsed.ImageUrls),
                Nutrition = ParseNutrition(parsed.Nutrition)
            };

            return recipesDto;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize OpenAI response. JSON parsing error at line {ex.LineNumber}, position {ex.BytePositionInLine}. " +
                $"Response may be incomplete or truncated. Raw response (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse OpenAI response: {ex.Message}. Raw response (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}", ex);
        }
    }

    private static ICollection<IngredientsDto> ParseIngredients(List<OpenAiIngredient>? ingredients)
    {
        var result = new List<IngredientsDto>();

        if (ingredients == null || ingredients.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No ingredients found in OpenAI response");
            return result;
        }

        System.Diagnostics.Debug.WriteLine($"Parsing {ingredients.Count} ingredients");

        foreach (var ingredient in ingredients)
        {
            System.Diagnostics.Debug.WriteLine($"  Ingredient: {ingredient.Name} - Qty: {ingredient.Quantity} {ingredient.Unit}");
            result.Add(new IngredientsDto
            {
                Name = ingredient.Name ?? "",
                Quantity = ingredient.Quantity > 0 ? ingredient.Quantity : 1,
                Unit = ingredient.Unit ?? ""
            });
        }

        return result;
    }

    private static ICollection<InstructionsDto> ParseInstructions(List<string>? instructions)
    {
        var result = new List<InstructionsDto>();

        if (instructions == null || instructions.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No instructions found in OpenAI response");
            return result;
        }

        System.Diagnostics.Debug.WriteLine($"Parsing {instructions.Count} instructions");

        for (int i = 0; i < instructions.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"  Step {i + 1}: {instructions[i]}");
            result.Add(new InstructionsDto
            {
                Step = instructions[i] ?? "",
                Order = i + 1
            });
        }

        return result;
    }

    private static ICollection<RecipeImageDto> ParseImages(List<string>? imageUrls)
    {
        var result = new List<RecipeImageDto>();

        if (imageUrls == null || imageUrls.Count == 0)
            return result;

        for (int i = 0; i < imageUrls.Count; i++)
        {
            var url = imageUrls[i]?.Trim();
            if (!string.IsNullOrWhiteSpace(url) && IsValidImageUrl(url))
            {
                result.Add(new RecipeImageDto
                {
                    Url = url,
                    FileName = ExtractFileNameFromUrl(url),
                    ContentType = GetContentTypeFromUrl(url),
                    IsPrimary = i == 0 // First image is primary
                });
            }
        }

        return result;
    }

    private static bool IsValidImageUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
            return false;

        return result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
    }

    private static string ExtractFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = System.IO.Path.GetFileName(uri.LocalPath);
            return !string.IsNullOrWhiteSpace(fileName) ? fileName : "image";
        }
        catch
        {
            return "image";
        }
    }

    private static string GetContentTypeFromUrl(string url)
    {
        try
        {
            var fileName = ExtractFileNameFromUrl(url);
            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
        catch
        {
            return "image/jpeg";
        }
    }

    private static NutritionDto? ParseNutrition(OpenAiNutrition? nutrition)
    {
        if (nutrition == null)
            return null;

        return new NutritionDto
        {
            Calories = nutrition.Calories > 0 ? nutrition.Calories : 0,
            Protein = nutrition.Protein > 0 ? nutrition.Protein : 0,
            Carbohydrates = nutrition.Carbohydrates > 0 ? nutrition.Carbohydrates : 0,
            Fat = nutrition.Fat > 0 ? nutrition.Fat : 0,
            Fiber = nutrition.Fiber > 0 ? nutrition.Fiber : 0,
            Sugar = nutrition.Sugar > 0 ? nutrition.Sugar : 0,
            Sodium = nutrition.Sodium,
            Potassium = nutrition.Potassium,
            Calcium = nutrition.Calcium,
            Iron = nutrition.Iron,
            Vitamin_A = nutrition.Vitamin_A,
            Vitamin_C = nutrition.Vitamin_C,
            Vitamin_D = nutrition.Vitamin_D,
            Cholesterol = nutrition.Cholesterol
        };
    }

    // Helper classes for OpenAI response mapping
    private class OpenAiRecipeResponse
    {
        public string? Title { get; set; }
        public string? MainCategory { get; set; }
        public string? SubCategory { get; set; }
        public string? Description { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<OpenAiIngredient>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
        public OpenAiNutrition? Nutrition { get; set; }
    }

    private class OpenAiIngredient
    {
        public string? Name { get; set; }
        public double Quantity { get; set; }
        public string? Unit { get; set; }
    }

    private class OpenAiNutrition
    {
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Fat { get; set; }
        public decimal Fiber { get; set; }
        public decimal Sugar { get; set; }
        public decimal? Sodium { get; set; }
        public decimal? Potassium { get; set; }
        public decimal? Calcium { get; set; }
        public decimal? Iron { get; set; }
        public decimal? Vitamin_A { get; set; }
        public decimal? Vitamin_C { get; set; }
        public decimal? Vitamin_D { get; set; }
        public decimal? Cholesterol { get; set; }
    }
}
