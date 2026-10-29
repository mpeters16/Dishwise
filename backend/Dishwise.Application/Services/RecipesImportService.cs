using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using HtmlAgilityPack;
using Dishwise.Application.Dto;
using Dishwise.Application.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;


namespace Dishwise.Application.Services;

public class RecipesImportService : IRecipesImportService
{
    private readonly IOpenAiService _openAiService;
    private readonly IRecipeServices _recipeService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IBlobServices _blobService;
    private readonly ILogger<RecipesImportService> _logger;
    private readonly IMapper _mapper;
    private readonly HttpClient _http;

    public RecipesImportService(
        IOpenAiService openAiService,
        IRecipeServices recipeService,
        ICategoryRepository categoryRepository,
        IRecipeRepository recipeRepository,
        IBlobServices blobService,
        ILogger<RecipesImportService> logger,
        IMapper mapper)
    {
        _openAiService = openAiService;
        _recipeService = recipeService;
        _categoryRepository = categoryRepository;
        _recipeRepository = recipeRepository;
        _blobService = blobService;
        _logger = logger;
        _mapper = mapper;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
    }

    /// <summary>
    /// Main import method: fetches URL, extracts JSON-LD structured data, 
    /// falls back to OpenAI for missing data, and saves the recipe.
    /// </summary>
    public async Task<RecipesDto> ImportRecipeFromUrlAsync(
        string url,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting recipe import from {Url}", url);

        // Validate URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format", nameof(url));

        // Stop before fetching the page if this recipe was already imported
        var normalizedUrl = RecipeSourceUrl.Normalize(url);
        var existing = (await _recipeRepository.GetRecipesWithSourceUrlAsync())
            .FirstOrDefault(r => RecipeSourceUrl.Normalize(r.SourceUrl) == normalizedUrl);
        if (existing != null)
        {
            _logger.LogInformation("Import skipped: {Url} already imported as '{Title}'", url, existing.Title);
            throw new DuplicateRecipeException(existing.Id, existing.Title);
        }

        // Fetch HTML
        var htmlContent = await FetchHtmlAsync(uri, cancellationToken);
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new Exception("Failed to fetch content from the provided URL.");

        _logger.LogInformation("Fetched {Size} bytes of HTML from {Url}", htmlContent.Length, url);

        // Early check: is this a collection/listing page rather than a single recipe?
        if (IsCollectionPage(htmlContent))
            throw new Exception("The URL appears to be a recipe collection or listing page, not a single recipe. Please provide a direct link to a single recipe page.");

        // Step 1: Try JSON-LD structured data (most reliable)
        var recipeDto = TryExtractFromJsonLd(htmlContent);

        // Step 2: Extract images from HTML (og:image, img tags, etc.)
        var imageUrls = ExtractImageUrls(htmlContent);

        // Step 3: If JSON-LD worked, fill any gaps with OpenAI
        if (recipeDto != null && !string.IsNullOrWhiteSpace(recipeDto.Title))
        {
            _logger.LogInformation("Extracted recipe '{Title}' from JSON-LD structured data", recipeDto.Title);

            // Always merge HTML images on top of JSON-LD images (deduplicating by base path).
            // JSON-LD often only lists one canonical image while og:image / srcset contain
            // additional recipe photos that are valuable to keep.
            MergeHtmlImages(recipeDto, imageUrls);

            // Use OpenAI to enhance/fill missing fields if needed
            if (HasMissingFields(recipeDto))
            {
                _logger.LogInformation("JSON-LD data incomplete, using OpenAI to fill gaps");
                var extractedContent = ExtractRecipeContentFromHtml(htmlContent, imageUrls);
                var aiRecipe = await _openAiService.ParseRecipeFromHtmlAsync(extractedContent, cancellationToken);
                MergeRecipeData(recipeDto, aiRecipe);
            }
        }
        else
        {
            // Step 4: No JSON-LD found — fall back entirely to OpenAI
            _logger.LogInformation("No JSON-LD found, falling back to OpenAI parsing");
            var extractedContent = ExtractRecipeContentFromHtml(htmlContent, imageUrls);
            recipeDto = await _openAiService.ParseRecipeFromHtmlAsync(extractedContent, cancellationToken);

            // Merge HTML images (covers case where OpenAI returned none or fewer)
            MergeHtmlImages(recipeDto, imageUrls);
        }

        if (recipeDto == null || string.IsNullOrWhiteSpace(recipeDto.Title))
            throw new Exception("Could not extract recipe data from the provided URL. Make sure it's a single recipe page, not a collection or listing page.");

        _logger.LogInformation("Recipe parsed: '{Title}', {IngCount} ingredients, {InsCount} instructions, {ImgCount} images",
            recipeDto.Title,
            recipeDto.Ingredients?.Count ?? 0,
            recipeDto.Instructions?.Count ?? 0,
            recipeDto.Images?.Count ?? 0);

        // Resolve category
        var categoryId = await ResolveCategoryAsync(recipeDto.MainCategoryName, recipeDto.SubCategoryName, recipeDto.Title);
        recipeDto.UserId = userId;
        recipeDto.CategoryId = categoryId;
        recipeDto.SourceUrl = url;

        // Save
        ApplyHeroImage(recipeDto, htmlContent);
        recipeDto.Ingredients = IngredientParser.Deduplicate(recipeDto.Ingredients ?? []);

        await _recipeService.CreateRecipeAsync(recipeDto);
        _logger.LogInformation("Recipe '{Title}' saved successfully", recipeDto.Title);

        return recipeDto;
    }

    /// <summary>
    /// Import from raw text using OpenAI.
    /// </summary>
    public async Task<RecipesDto> ImportRecipeFromTextAsync(
        string rawText,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting recipe import from raw text");

        if (string.IsNullOrWhiteSpace(rawText))
            throw new ArgumentException("Recipe text cannot be empty", nameof(rawText));

        var recipeDto = await _openAiService.ParseRecipeAsync(rawText, cancellationToken);

        var categoryId = await ResolveCategoryAsync(recipeDto.MainCategoryName, recipeDto.SubCategoryName, recipeDto.Title);
        recipeDto.UserId = userId;
        recipeDto.CategoryId = categoryId;

        recipeDto.Ingredients = IngredientParser.Deduplicate(recipeDto.Ingredients ?? []);

        await _recipeService.CreateRecipeAsync(recipeDto);
        _logger.LogInformation("Recipe '{Title}' imported from text", recipeDto.Title);

        return recipeDto;
    }

    // =============================
    // JSON-LD Extraction
    // =============================

    /// <summary>
    /// Extracts recipe data from JSON-LD schema.org/Recipe structured data.
    /// Most major recipe sites (BBC Good Food, AllRecipes, etc.) include this.
    /// </summary>
    private RecipesDto? TryExtractFromJsonLd(string htmlContent)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes == null) return null;

            // Collect all JSON-LD elements across all script blocks (needed for @graph that spans multiple blocks)
            var allElements = new List<JsonElement>();
            foreach (var node in jsonLdNodes)
            {
                var json = node.InnerText.Trim();
                if (string.IsNullOrWhiteSpace(json)) continue;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var el = JsonSerializer.Deserialize<JsonElement>(json, options);
                    // Flatten @graph arrays
                    if (el.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
                        foreach (var item in graph.EnumerateArray()) allElements.Add(item);
                    else if (el.ValueKind == JsonValueKind.Array)
                        foreach (var item in el.EnumerateArray()) allElements.Add(item);
                    else
                        allElements.Add(el);
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug(ex, "Failed to parse JSON-LD block");
                }
            }

            // Extract breadcrumb category from BreadcrumbList if present
            string? breadcrumbCategory = ExtractBreadcrumbCategory(allElements);

            // Find the Recipe element
            JsonElement? recipeJson = null;
            foreach (var el in allElements)
            {
                if (IsRecipeType(el)) { recipeJson = el; break; }
            }

            if (recipeJson == null) return null;

            var dto = MapJsonLdToRecipeDto(recipeJson.Value);

            // Breadcrumbs are a weaker signal than schema.org recipeCategory, so only use them when that is missing
            if (dto.MainCategoryName == null && !string.IsNullOrWhiteSpace(breadcrumbCategory))
            {
                dto.MainCategoryName = MealCategories.NormalizeMain(breadcrumbCategory);
                dto.SubCategoryName ??= breadcrumbCategory;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting JSON-LD data");
        }

        return null;
    }

    /// <summary>
    /// Extracts the most relevant category name from BreadcrumbList JSON-LD elements.
    /// Returns the last meaningful breadcrumb item (e.g. "Cakes" from Home > Cakes > Birthday Cake).
    /// </summary>
    private string? ExtractBreadcrumbCategory(List<JsonElement> elements)
    {
        foreach (var el in elements)
        {
            if (!el.TryGetProperty("@type", out var type)) continue;
            var typeStr = type.ValueKind == JsonValueKind.String ? type.GetString() : type.ToString();
            if (typeStr?.Contains("BreadcrumbList", StringComparison.OrdinalIgnoreCase) != true) continue;

            if (!el.TryGetProperty("itemListElement", out var items) || items.ValueKind != JsonValueKind.Array)
                continue;

            string? last = null;
            foreach (var item in items.EnumerateArray())
            {
                // item.name or item.item.name
                var name = GetJsonString(item, "name");
                if (string.IsNullOrWhiteSpace(name) && item.TryGetProperty("item", out var inner))
                    name = GetJsonString(inner, "name");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    // Skip generic breadcrumbs
                    var lower = name.Trim().ToLowerInvariant();
                    if (lower is "home" or "recipes" or "all recipes" or "recipe index")
                        continue;
                    last = name.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(last))
                return last;
        }
        return null;
    }

    /// <summary>
    /// Finds the Recipe object within JSON-LD (may be top-level, in @graph array, or nested).
    /// </summary>
    private JsonElement? FindRecipeInJsonLd(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var element = JsonSerializer.Deserialize<JsonElement>(json, options);

        // Check if it's directly a Recipe
        if (IsRecipeType(element))
            return element;

        // Check @graph array
        if (element.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in graph.EnumerateArray())
            {
                if (IsRecipeType(item))
                    return item;
            }
        }

        // Check if it's an array of objects
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (IsRecipeType(item))
                    return item;
            }
        }

        return null;
    }

    private bool IsRecipeType(JsonElement element)
    {
        if (element.TryGetProperty("@type", out var typeValue))
        {
            if (typeValue.ValueKind == JsonValueKind.String)
                return typeValue.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true;

            if (typeValue.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in typeValue.EnumerateArray())
                {
                    if (t.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Maps JSON-LD Recipe schema to RecipesDto.
    /// </summary>
    private RecipesDto MapJsonLdToRecipeDto(JsonElement recipe)
    {
        var (instructions, stepImages) = ExtractJsonLdInstructions(recipe);

        var dto = new RecipesDto
        {
            Title = GetJsonString(recipe, "name") ?? "",
            Description = GetJsonString(recipe, "description") ?? "",
            Ingredients = ExtractJsonLdIngredients(recipe),
            Instructions = instructions,
            Images = ExtractJsonLdImages(recipe),
            Nutrition = ExtractJsonLdNutrition(recipe),
        };

        // Merge step images into the image list (they'll be added later if the main image list is empty)
        if (stepImages.Count > 0)
        {
            foreach (var si in stepImages)
                dto.Images.Add(si);
        }

        // Parse prep/cook times (ISO 8601 duration format: PT30M, PT1H30M)
        dto.PrepTimeMinutes = ParseIsoDuration(GetJsonString(recipe, "prepTime"));
        dto.CookTimeMinutes = ParseIsoDuration(GetJsonString(recipe, "cookTime"));

        // If no prep time but total time exists, use that
        if (dto.PrepTimeMinutes == 0 && dto.CookTimeMinutes == 0)
        {
            var totalTime = ParseIsoDuration(GetJsonString(recipe, "totalTime"));
            if (totalTime > 0) dto.CookTimeMinutes = totalTime;
        }

        // schema.org recipeCategory is the most explicit meal label; recipeCuisine becomes the subcategory
        dto.MainCategoryName = FirstMainCategory(recipe, "recipeCategory");
        dto.SubCategoryName = FirstString(recipe, "recipeCuisine");

        return dto;
    }

    /// <summary>
    /// Reads a JSON-LD property that may be a string or an array of strings, returning the first non-empty value.
    /// </summary>
    private static string? FirstString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value)) return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Array => value.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.String)
                .Select(v => v.GetString())
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)),
            _ => null
        };
    }

    /// <summary>
    /// Returns the first JSON-LD category label that maps to a known meal category.
    /// </summary>
    private static string? FirstMainCategory(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value)) return null;

        IEnumerable<string?> labels = value.ValueKind switch
        {
            JsonValueKind.String => [value.GetString()],
            JsonValueKind.Array => value.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.String)
                .Select(v => v.GetString()),
            _ => []
        };

        return labels.Select(MealCategories.NormalizeMain).FirstOrDefault(m => m != null);
    }

    private List<IngredientsDto> ExtractJsonLdIngredients(JsonElement recipe)
    {
        var ingredients = new List<IngredientsDto>();

        if (!recipe.TryGetProperty("recipeIngredient", out var ingredientArray))
            return ingredients;

        if (ingredientArray.ValueKind != JsonValueKind.Array)
            return ingredients;

        foreach (var item in ingredientArray.EnumerateArray())
        {
            var text = item.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;

            var (quantity, unit, name) = ParseIngredientText(text);
            ingredients.Add(new IngredientsDto
            {
                Name = name,
                Quantity = quantity,
                Unit = unit
            });
        }

        return ingredients;
    }

    /// <summary>
    /// Parses an ingredient line like "2 cups flour" into quantity, unit and name.
    /// </summary>
    private static (double quantity, string? unit, string name) ParseIngredientText(string text) =>
        IngredientParser.Parse(text);

    private (List<InstructionsDto> instructions, List<RecipeImageDto> stepImages) ExtractJsonLdInstructions(JsonElement recipe)
    {
        var instructions = new List<InstructionsDto>();
        var stepImages = new List<RecipeImageDto>();

        if (!recipe.TryGetProperty("recipeInstructions", out var instructionArray))
            return (instructions, stepImages);

        int order = 1;

        if (instructionArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in instructionArray.EnumerateArray())
            {
                string? stepText = null;

                if (item.ValueKind == JsonValueKind.String)
                {
                    stepText = item.GetString();
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    // HowToStep or HowToSection
                    stepText = GetJsonString(item, "text") ?? GetJsonString(item, "name");

                    // Handle HowToSection with itemListElement
                    if (item.TryGetProperty("itemListElement", out var subSteps) &&
                        subSteps.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var subStep in subSteps.EnumerateArray())
                        {
                            var subText = GetJsonString(subStep, "text") ?? subStep.GetString();
                            if (!string.IsNullOrWhiteSpace(subText))
                            {
                                instructions.Add(new InstructionsDto { Step = subText.Trim(), Order = order });
                                TryExtractStepImage(subStep, order, stepImages);
                                order++;
                            }
                        }
                        continue;
                    }

                    // Extract per-step image from HowToStep
                    TryExtractStepImage(item, order, stepImages);
                }

                if (!string.IsNullOrWhiteSpace(stepText))
                {
                    instructions.Add(new InstructionsDto { Step = stepText.Trim(), Order = order++ });
                }
            }
        }
        else if (instructionArray.ValueKind == JsonValueKind.String)
        {
            // Some sites put all instructions as a single string
            var text = instructionArray.GetString() ?? "";
            var steps = text.Split(new[] { '\n', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var step in steps)
            {
                var trimmed = step.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    instructions.Add(new InstructionsDto { Step = trimmed, Order = order++ });
            }
        }

        return (instructions, stepImages);
    }

    /// <summary>
    /// Attempts to extract an image from a HowToStep JSON element and adds it to the step images list.
    /// </summary>
    private void TryExtractStepImage(JsonElement stepElement, int stepOrder, List<RecipeImageDto> stepImages)
    {
        if (!stepElement.TryGetProperty("image", out var imageValue)) return;

        string? url = null;

        if (imageValue.ValueKind == JsonValueKind.String)
        {
            url = imageValue.GetString();
        }
        else if (imageValue.ValueKind == JsonValueKind.Object)
        {
            url = GetJsonString(imageValue, "url") ?? GetJsonString(imageValue, "contentUrl");
        }
        else if (imageValue.ValueKind == JsonValueKind.Array)
        {
            foreach (var img in imageValue.EnumerateArray())
            {
                if (img.ValueKind == JsonValueKind.String)
                    url = img.GetString();
                else if (img.ValueKind == JsonValueKind.Object)
                    url = GetJsonString(img, "url") ?? GetJsonString(img, "contentUrl");
                if (!string.IsNullOrWhiteSpace(url)) break;
            }
        }

        if (IsValidImageUrl(url))
        {
            stepImages.Add(new RecipeImageDto { Url = url!, StepOrder = stepOrder });
        }
    }

    private List<RecipeImageDto> ExtractJsonLdImages(JsonElement recipe)
    {
        var images = new List<RecipeImageDto>();

        // Deduplicate by base path (strip query strings like ?resize=1200&zoom=0.54)
        // and keep the highest-res variant of each unique image path.
        var candidates = new List<string>();

        void CollectUrl(string? u)
        {
            if (IsValidImageUrl(u)) candidates.Add(u!);
        }

        // Primary "image" field
        if (recipe.TryGetProperty("image", out var imageValue))
        {
            switch (imageValue.ValueKind)
            {
                case JsonValueKind.String:
                    CollectUrl(imageValue.GetString());
                    break;

                case JsonValueKind.Array:
                    foreach (var item in imageValue.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                            CollectUrl(item.GetString());
                        else if (item.ValueKind == JsonValueKind.Object)
                            CollectUrl(GetJsonString(item, "url") ?? GetJsonString(item, "contentUrl"));
                    }
                    break;

                case JsonValueKind.Object:
                    CollectUrl(GetJsonString(imageValue, "url") ?? GetJsonString(imageValue, "contentUrl"));
                    break;
            }
        }

        // Also pick up "thumbnailUrl" (some sites put a different photo here)
        if (recipe.TryGetProperty("thumbnailUrl", out var thumbValue))
        {
            if (thumbValue.ValueKind == JsonValueKind.String)
                CollectUrl(thumbValue.GetString());
            else if (thumbValue.ValueKind == JsonValueKind.Array)
                foreach (var t in thumbValue.EnumerateArray())
                    CollectUrl(t.ValueKind == JsonValueKind.String ? t.GetString() : GetJsonString(t, "url"));
        }

        // Deduplicate by base path; prefer highest numeric width in query string
        var bestByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in candidates)
        {
            var basePath = GetBaseImagePath(url);
            if (!bestByPath.TryGetValue(basePath, out var existing) || GetImageWidth(url) > GetImageWidth(existing))
                bestByPath[basePath] = url;
        }

        foreach (var url in bestByPath.Values)
            images.Add(new RecipeImageDto { Url = url });

        return images;
    }

    private NutritionDto? ExtractJsonLdNutrition(JsonElement recipe)
    {
        if (!recipe.TryGetProperty("nutrition", out var nutrition))
            return null;

        if (nutrition.ValueKind != JsonValueKind.Object)
            return null;

        var dto = new NutritionDto();
        var hasAnyValue = false;

        if (TryParseNutrientValue(GetJsonString(nutrition, "calories"), out var cal))
        { dto.Calories = cal; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "proteinContent"), out var protein))
        { dto.Protein = protein; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "carbohydrateContent"), out var carbs))
        { dto.Carbohydrates = carbs; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "fatContent"), out var fat))
        { dto.Fat = fat; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "fiberContent"), out var fiber))
        { dto.Fiber = fiber; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "sugarContent"), out var sugar))
        { dto.Sugar = sugar; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "sodiumContent"), out var sodium))
        { dto.Sodium = sodium; hasAnyValue = true; }

        if (TryParseNutrientValue(GetJsonString(nutrition, "cholesterolContent"), out var chol))
        { dto.Cholesterol = chol; hasAnyValue = true; }

        return hasAnyValue ? dto : null;
    }

    // =============================
    // HTML Image Extraction
    // =============================

    /// <summary>
    /// Extracts image URLs from HTML using multiple strategies:
    /// og:image meta tags, img src/data-src, srcset, etc.
    /// </summary>
    private List<string> ExtractImageUrls(string htmlContent)
    {
        var candidates = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // 1. Open Graph image (most reliable for the main recipe image)
        var ogUrl = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "");
        if (IsValidImageUrl(ogUrl)) candidates.Add(ogUrl!);

        // 2. Twitter card image
        var twitterUrl = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", "");
        if (IsValidImageUrl(twitterUrl)) candidates.Add(twitterUrl!);

        // 3. All og:image:* variants (some sites list multiple)
        var ogImageNodes = doc.DocumentNode.SelectNodes("//meta[starts-with(@property,'og:image')]");
        if (ogImageNodes != null)
        {
            foreach (var node in ogImageNodes)
            {
                var content = node.GetAttributeValue("content", "");
                if (IsValidImageUrl(content)) candidates.Add(content);
            }
        }

        // 4. Recipe container images — search inside known recipe wrappers first,
        //    then fall back to the whole page so gallery images outside the card are caught too.
        var searchRoots = new List<HtmlAgilityPack.HtmlNode>();
        var recipeContainerNodes = doc.DocumentNode.SelectNodes(
            "//*[contains(@class,'recipe')] | //*[@itemtype='https://schema.org/Recipe'] | //article | //main");
        if (recipeContainerNodes != null)
            searchRoots.AddRange(recipeContainerNodes);
        // Always also scan the full page to catch images in galleries / carousels outside the recipe card
        searchRoots.Add(doc.DocumentNode);

        foreach (var root in searchRoots)
        {
            var imgNodes = root.SelectNodes(".//img");
            if (imgNodes == null) continue;

            foreach (var img in imgNodes)
            {
                // src / lazy-load variants
                foreach (var attr in new[] { "src", "data-src", "data-lazy-src", "data-original", "data-lazy", "data-lazyload" })
                {
                    var src = img.GetAttributeValue(attr, "");
                    if (IsValidImageUrl(src)) { candidates.Add(src); break; }
                }

                // srcset — keep best resolution entry
                var srcset = img.GetAttributeValue("srcset", "");
                if (!string.IsNullOrWhiteSpace(srcset))
                {
                    var bestUrl = GetBestSrcsetUrl(srcset);
                    if (IsValidImageUrl(bestUrl)) candidates.Add(bestUrl!);
                }

                // data-srcset (lazy loading pattern)
                var dataSrcset = img.GetAttributeValue("data-srcset", "");
                if (!string.IsNullOrWhiteSpace(dataSrcset))
                {
                    var bestUrl = GetBestSrcsetUrl(dataSrcset);
                    if (IsValidImageUrl(bestUrl)) candidates.Add(bestUrl!);
                }
            }
        }

        // Filter non-recipe images, deduplicate by base path keeping highest-res variant
        // Attribute values can still hold HTML entities (e.g. "&amp;zoom=1"), which break the image request
        var filtered = candidates
            .Select(u => WebUtility.HtmlDecode(u))
            .Where(u => !IsNonRecipeImage(u))
            .ToList();
        var bestByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in filtered)
        {
            var basePath = GetBaseImagePath(url);
            if (!bestByPath.TryGetValue(basePath, out var existing) || GetImageWidth(url) > GetImageWidth(existing))
                bestByPath[basePath] = url;
        }

        // Increased from 10 → 20 to capture all meaningful recipe photos
        return bestByPath.Values.Take(20).ToList();
    }

    /// <summary>
    /// Gets the highest resolution URL from a srcset attribute.
    /// </summary>
    private string? GetBestSrcsetUrl(string srcset)
    {
        var entries = srcset.Split(',', StringSplitOptions.RemoveEmptyEntries);
        string? bestUrl = null;
        int bestWidth = 0;

        foreach (var entry in entries)
        {
            var parts = entry.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
            {
                var url = parts[0];
                var width = 0;
                if (parts.Length >= 2)
                {
                    var descriptor = parts[1].TrimEnd('w', 'x');
                    int.TryParse(descriptor, out width);
                }

                if (width > bestWidth || bestUrl == null)
                {
                    bestUrl = url;
                    bestWidth = width;
                }
            }
        }

        return bestUrl;
    }

    /// <summary>
    /// Makes the page's hero image (og:image, falling back to twitter:image) the first image and the only primary one.
    /// Recipe sites use this image for sharing, so it is the photo that best represents the recipe.
    /// </summary>
    private void ApplyHeroImage(RecipesDto recipeDto, string htmlContent)
    {
        var images = (recipeDto.Images ?? new List<RecipeImageDto>()).ToList();

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        var heroRaw = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "")
            ?? doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", "");
        var hero = WebUtility.HtmlDecode(heroRaw ?? "");

        if (!IsValidImageUrl(hero))
        {
            // No hero declared: the first image is the primary one
            foreach (var image in images) image.IsPrimary = image == images.FirstOrDefault();
            recipeDto.Images = images;
            return;
        }

        var heroPath = GetBaseImagePath(hero);
        var heroImage = images.FirstOrDefault(i => GetBaseImagePath(i.Url ?? "") == heroPath) ?? new RecipeImageDto();
        images.Remove(heroImage);

        // Use the full-size URL from the page, not a resized variant
        heroImage.Url = hero;
        images.Insert(0, heroImage);

        foreach (var image in images) image.IsPrimary = image == heroImage;
        recipeDto.Images = images;
    }

    /// <summary>
    /// Merges HTML-scraped image URLs into the recipe DTO's image list,
    /// deduplicating against already-present images by base path.
    /// Step images (StepOrder != null) are never replaced by HTML images.
    /// </summary>
    private void MergeHtmlImages(RecipesDto recipeDto, List<string> htmlImageUrls)
    {
        if (htmlImageUrls.Count == 0) return;

        recipeDto.Images ??= new List<RecipeImageDto>();

        // Build a set of base paths already in the list so we don't add duplicates
        var existingPaths = new HashSet<string>(
            recipeDto.Images.Select(i => GetBaseImagePath(i.Url ?? "")),
            StringComparer.OrdinalIgnoreCase);

        // Track best (highest-res) candidate per base path for new URLs
        var newByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in htmlImageUrls)
        {
            var basePath = GetBaseImagePath(url);
            if (existingPaths.Contains(basePath)) continue; // already present from JSON-LD

            if (!newByPath.TryGetValue(basePath, out var existing) || GetImageWidth(url) > GetImageWidth(existing))
                newByPath[basePath] = url;
        }

        foreach (var url in newByPath.Values)
            recipeDto.Images.Add(new RecipeImageDto { Url = url });

        _logger.LogInformation("After HTML image merge: {Count} total images", recipeDto.Images.Count);
    }

    private bool IsNonRecipeImage(string url)
    {
        var lower = url.ToLowerInvariant();
        // Site chrome, promos and nutrition charts are not photos of the dish
        var nonRecipePatterns = new[]
        {
            "logo", "icon", "avatar", "badge", "sprite", "pixel", "tracking", "ad-", "advertisement", "1x1", "blank.gif",
            "/themes/", "/assets/", "nutrition", "popup", "cookbook", "ebook"
        };
        return nonRecipePatterns.Any(p => lower.Contains(p));
    }

    /// <summary>
    /// Returns the base path of an image URL without query string, for deduplication.
    /// e.g. "https://example.com/img/cake.jpg?resize=1200&zoom=0.5" → "example.com/img/cake.jpg"
    /// </summary>
    private static string GetBaseImagePath(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return url;
        return (uri.Host + uri.AbsolutePath).ToLowerInvariant();
    }

    /// <summary>
    /// Extracts the numeric width from a URL query string (e.g. ?resize=1200 → 1200, or w=900 → 900).
    /// Returns 0 if none found.
    /// </summary>
    private static int GetImageWidth(string url)
    {
        var match = System.Text.RegularExpressions.Regex.Match(url, @"(?:resize|width|w)=(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var w) ? w : 0;
    }

    /// <summary>
    /// Heuristically detects whether a page is a recipe collection/listing rather than a single recipe.
    /// Checks for absence of single-recipe JSON-LD and presence of collection-page signals.
    /// </summary>
    private bool IsCollectionPage(string htmlContent)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // If the page has a single Recipe JSON-LD block, it's not a collection
            var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes != null)
            {
                foreach (var node in jsonLdNodes)
                {
                    var json = node.InnerText.Trim();
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    try
                    {
                        var found = FindRecipeInJsonLd(json);
                        if (found != null) return false; // Has a single recipe — not a collection
                    }
                    catch { }
                }
            }

            // Check page title / h1 for collection signals
            var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.ToLowerInvariant() ?? "";
            var h1 = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.ToLowerInvariant() ?? "";
            var collectionKeywords = new[] { " recipes", "best recipes", "easy recipes", "collection", "top 10", "top 20", "top 25", "ideas for" };
            if (collectionKeywords.Any(k => title.Contains(k) || h1.Contains(k)))
            {
                // Additional confirmation: many recipe card links on the page
                var recipeLinks = doc.DocumentNode.SelectNodes("//*[contains(@class,'recipe-card') or contains(@class,'recipe-item') or contains(@class,'recipe-list')]");
                if (recipeLinks != null && recipeLinks.Count >= 3)
                    return true;

                // Or a list of many h2/h3 headings (typical of "20 best X recipes" articles)
                var headings = doc.DocumentNode.SelectNodes("//h2 | //h3");
                if (headings != null && headings.Count >= 8)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    // =============================
    // HTML Content Extraction (for OpenAI fallback)
    // =============================

    /// <summary>
    /// Extracts recipe-relevant content from HTML for OpenAI parsing.
    /// Includes text content and image URLs.
    /// </summary>
    private string ExtractRecipeContentFromHtml(string htmlContent, List<string> imageUrls)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Remove noise
        var nodesToRemove = doc.DocumentNode.SelectNodes(
            "//script | //style | //nav | //footer | //header | //noscript | //iframe | " +
            "//*[contains(@class, 'ad') or contains(@class, 'comment') or contains(@class, 'sidebar') or contains(@class, 'social')]");

        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        // Try to find recipe-specific container
        var recipeContainer = doc.DocumentNode.SelectSingleNode(
            "//*[@itemtype='https://schema.org/Recipe'] | " +
            "//*[contains(@class, 'recipe')] | " +
            "//article | //main") ?? doc.DocumentNode;

        // Extract text
        var text = recipeContainer.InnerText;
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // Append image URLs for OpenAI
        if (imageUrls.Count > 0)
        {
            text += "\n\n[RECIPE IMAGES]\n" + string.Join("\n", imageUrls);
        }

        // Limit to 12000 chars (increased from 8000 for better coverage)
        if (text.Length > 12000)
            text = text[..12000] + "\n[Content truncated]";

        return text;
    }

    // =============================
    // HTML Fetching
    // =============================

    private async Task<string> FetchHtmlAsync(Uri uri, CancellationToken cancellationToken)
    {
        // Try HttpClient first (faster, handles redirects)
        try
        {
            var response = await _http.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HttpClient fetch failed, trying HtmlAgilityPack for {Url}", uri);
        }

        // Fallback to HtmlAgilityPack (handles some edge cases better)
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        var doc = await web.LoadFromWebAsync(uri.AbsoluteUri);

        return doc?.DocumentNode?.OuterHtml
            ?? throw new Exception($"Failed to fetch content from {uri}");
    }

    // =============================
    // Category Parsing
    // =============================

    private async Task<Guid> ResolveCategoryAsync(string? mainCategoryName, string? subCategoryName, string? title)
    {
        // Top-level category: explicit label first, then the title, then the subcategory label
        var main = MealCategories.NormalizeMain(mainCategoryName)
            ?? MealCategories.InferMainFromTitle(title)
            ?? MealCategories.NormalizeMain(subCategoryName)
            ?? MealCategories.DefaultMain;

        var sub = MealCategories.CleanSub(subCategoryName) ?? MealCategories.DefaultSub;

        var mainCategory = await _categoryRepository.GetMainCategoryByNameAsync(main)
            ?? await CreateCategoryAsync(new Category { Name = main, ParentCategoryId = null });

        // Category names are unique across the whole table, so an existing category with this name is
        // reused only when it already sits under this meal. Otherwise the recipe is filed directly under the meal.
        var existing = await _categoryRepository.GetCategoryByNameAsync(sub);
        if (existing == null)
        {
            var created = await CreateCategoryAsync(new Category { Name = sub, ParentCategoryId = mainCategory.Id });
            _logger.LogInformation("Resolved category '{Main}' > '{Sub}'", main, sub);
            return created.Id;
        }

        if (existing.ParentCategoryId == mainCategory.Id)
        {
            _logger.LogInformation("Resolved category '{Main}' > '{Sub}'", main, sub);
            return existing.Id;
        }

        _logger.LogWarning("'{Sub}' already exists outside '{Main}'; filing the recipe under '{Main}'", sub, main, main);
        return mainCategory.Id;
    }

    private async Task<Category> CreateCategoryAsync(Category category)
    {
        await _categoryRepository.AddCategoryAsync(category);
        _logger.LogInformation("Created category '{Name}' (parent: {ParentId})", category.Name, category.ParentCategoryId);
        return category;
    }
    // =============================
    // Utility Methods
    // =============================

    private bool HasMissingFields(RecipesDto dto)
    {
        return string.IsNullOrWhiteSpace(dto.Title) ||
               dto.Ingredients == null || dto.Ingredients.Count == 0 ||
               dto.Instructions == null || dto.Instructions.Count == 0;
    }

    /// <summary>
    /// Merges AI-parsed data into the main DTO, filling only missing fields.
    /// </summary>
    private void MergeRecipeData(RecipesDto target, RecipesDto source)
    {
        if (string.IsNullOrWhiteSpace(target.Title))
            target.Title = source.Title;

        if (string.IsNullOrWhiteSpace(target.Description))
            target.Description = source.Description;

        if (target.Ingredients == null || target.Ingredients.Count == 0)
            target.Ingredients = source.Ingredients;

        if (target.Instructions == null || target.Instructions.Count == 0)
            target.Instructions = source.Instructions;

        if (target.PrepTimeMinutes == 0 && source.PrepTimeMinutes > 0)
            target.PrepTimeMinutes = source.PrepTimeMinutes;

        if (target.CookTimeMinutes == 0 && source.CookTimeMinutes > 0)
            target.CookTimeMinutes = source.CookTimeMinutes;

        if (target.Nutrition == null)
            target.Nutrition = source.Nutrition;

        if ((target.Images == null || target.Images.Count == 0) && source.Images?.Count > 0)
            target.Images = source.Images;

        if (string.IsNullOrWhiteSpace(target.MainCategoryName))
            target.MainCategoryName = source.MainCategoryName;

        if (string.IsNullOrWhiteSpace(target.SubCategoryName))
            target.SubCategoryName = source.SubCategoryName;
    }

    private int ParseIsoDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration)) return 0;

        var match = Regex.Match(duration, @"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?", RegexOptions.IgnoreCase);
        if (!match.Success) return 0;

        var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

        return (hours * 60) + minutes;
    }

    private bool TryParseNutrientValue(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Extract number from strings like "250 kcal", "12.5g", "25 g"
        var match = Regex.Match(text, @"([\d.]+)");
        return match.Success && decimal.TryParse(match.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    private string? GetJsonString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value))
        {
            return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        }
        return null;
    }

    private bool IsValidImageUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) &&
               Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == "http" || uri.Scheme == "https");
    }

}
