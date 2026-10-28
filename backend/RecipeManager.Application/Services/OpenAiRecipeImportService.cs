using System.Text.Json;
using HtmlAgilityPack;
using RecipeManager.Application.Dto;
using RecipeManager.Application.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace RecipeManager.Application.Services;

public class OpenAiRecipeImportService : IOpenAiRecipeImportService
{
    private readonly IOpenAiService _openAiService;
    private readonly IRecipeServices _recipeService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<OpenAiRecipeImportService> _logger;
    private readonly IMapper _mapper;

    public OpenAiRecipeImportService(
        IOpenAiService openAiService,
        IRecipeServices recipeService,
        ICategoryRepository categoryRepository,
        ILogger<OpenAiRecipeImportService> logger,
        IMapper mapper)
    {
        _openAiService = openAiService;
        _recipeService = recipeService;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Imports a recipe from a URL using OpenAI to parse and enhance the recipe content.
    /// </summary>
    public async Task<RecipesDto> ImportRecipeFromUrlWithOpenAiAsync(
        string url,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting OpenAI-powered recipe import from {Url}", url);

            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL format", nameof(url));

            // Fetch the HTML directly
            var htmlContent = await FetchHtmlContentAsync(uri, cancellationToken);

            if (string.IsNullOrWhiteSpace(htmlContent))
                throw new Exception("Failed to fetch HTML content from the provided URL.");

            _logger.LogInformation("Fetched HTML content ({HtmlSize} bytes), sending directly to OpenAI for parsing", htmlContent.Length);

            // Extract recipe content from HTML to reduce OpenAI token count
            var recipeContent = ExtractRecipeContentFromHtml(htmlContent);
            
            if (string.IsNullOrWhiteSpace(recipeContent))
                throw new Exception("No recipe content found on the webpage.");

            _logger.LogInformation("Extracted recipe content ({ExtractedSize} bytes) from HTML for OpenAI parsing", recipeContent.Length);

            // Send extracted recipe content to OpenAI for parsing (includes images, ingredients, instructions, nutrition)
            var recipeDto = await _openAiService.ParseRecipeFromHtmlAsync(recipeContent, cancellationToken);

            _logger.LogInformation("Recipe parsed successfully: {Title}, Ingredients: {IngredientCount}, Instructions: {InstructionCount}, Images: {ImageCount}", 
                recipeDto.Title, 
                recipeDto.Ingredients?.Count ?? 0,
                recipeDto.Instructions?.Count ?? 0,
                recipeDto.Images?.Count ?? 0);


            // Get or create hierarchical categories (main and sub)
            Guid categoryId;
            try
            {
                categoryId = await GetOrCreateHierarchicalCategoryAsync(
                    recipeDto.MainCategoryName, 
                    recipeDto.SubCategoryName);

                _logger.LogInformation("Resolved category ID: {CategoryId} for main: '{Main}', sub: '{Sub}'", 
                    categoryId, recipeDto.MainCategoryName, recipeDto.SubCategoryName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve categories (main: '{Main}', sub: '{Sub}'). Using fallback.", 
                    recipeDto.MainCategoryName, recipeDto.SubCategoryName);
                categoryId = await GetOrCreateHierarchicalCategoryAsync("Dinner", "Main Course");
            }

            if (categoryId == Guid.Empty)
                throw new Exception("Failed to resolve a valid category for the imported recipe.");

            // Set user and category IDs
            recipeDto.UserId = userId;
            recipeDto.CategoryId = categoryId;

            // Save the recipe
            await _recipeService.CreateRecipeAsync(recipeDto);

            _logger.LogInformation("Recipe '{RecipeTitle}' successfully imported from {Url}", recipeDto.Title, url);

            return recipeDto;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch URL: {Url}", url);
            throw new Exception($"Failed to fetch the URL: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse recipe JSON from OpenAI response");
            throw new Exception($"Failed to parse recipe data: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing recipe from {Url} with OpenAI", url);
            throw;
        }
    }

    /// <summary>
    /// Imports and enhances a recipe from raw text using OpenAI.
    /// </summary>
    public async Task<RecipesDto> ImportRecipeFromTextWithOpenAiAsync(
        string rawRecipeText,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting OpenAI-powered recipe import from raw text");

            if (string.IsNullOrWhiteSpace(rawRecipeText))
                throw new ArgumentException("Recipe text cannot be empty", nameof(rawRecipeText));

            // Use OpenAI to parse the recipe
            var recipeDto = await _openAiService.ParseRecipeAsync(rawRecipeText, cancellationToken);

            // Get or create hierarchical categories (main and sub)
            Guid resolvedCategoryId;
            try
            {
                resolvedCategoryId = await GetOrCreateHierarchicalCategoryAsync(
                    recipeDto.MainCategoryName, 
                    recipeDto.SubCategoryName);
                _logger.LogInformation("Resolved category ID: {CategoryId} for main: '{Main}', sub: '{Sub}'", 
                    resolvedCategoryId, recipeDto.MainCategoryName, recipeDto.SubCategoryName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve categories (main: '{Main}', sub: '{Sub}'). Using fallback.", 
                    recipeDto.MainCategoryName, recipeDto.SubCategoryName);
                resolvedCategoryId = await GetOrCreateHierarchicalCategoryAsync("Dinner", "Main Course");
            }

            if (resolvedCategoryId == Guid.Empty)
                throw new Exception("Failed to resolve a valid category for the imported recipe.");

            // Set user and category IDs
            recipeDto.UserId = userId;
            recipeDto.CategoryId = resolvedCategoryId;

            // Save the recipe
            await _recipeService.CreateRecipeAsync(recipeDto);

            _logger.LogInformation("Recipe '{RecipeTitle}' successfully imported from raw text", recipeDto.Title);

            return recipeDto;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse recipe JSON from OpenAI response");
            throw new Exception($"Failed to parse recipe data: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing recipe from raw text with OpenAI");
            throw;
        }
    }

    /// <summary>
    /// Fetches HTML content from a URL.
    /// </summary>
    private async Task<string> FetchHtmlContentAsync(Uri uri, CancellationToken cancellationToken)
    {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(uri.AbsoluteUri);

        if (doc == null)
            throw new Exception("Failed to load webpage");

        return doc.DocumentNode.OuterHtml;
    }

    /// <summary>
    /// Extracts readable text content from HTML, removing scripts, styles, and excessive whitespace.
    /// </summary>
    private string ExtractTextFromHtml(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Remove script and style elements
        foreach (var node in doc.DocumentNode.SelectNodes("//script | //style"))
        {
            node.Remove();
        }

        // Get all text nodes
        var text = doc.DocumentNode.InnerText;

        // Clean up excessive whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        text = text.Trim();

        // Remove common navigation and boilerplate text
        text = RemoveCommonBoilerplate(text);

        return text;
    }

    /// <summary>
    /// Removes common website boilerplate text that's not part of the actual recipe.
    /// </summary>
    private string RemoveCommonBoilerplate(string text)
    {
        var patterns = new[]
        {
            @"(Cookie|Privacy|Terms of Service|Terms & Conditions).*?(?=\s|$)",
            @"(Advertisement|Ad)",
            @"(Jump to recipe|Skip to.*?recipe)",
            @"Share (this|on) (Facebook|Twitter|Pinterest)",
            @"About the author.*",
            @"Related recipes.*",
            @"More from this author.*"
        };

        foreach (var pattern in patterns)
        {
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                pattern,
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return text;
    }

    /// <summary>
    /// Extracts recipe-specific content from HTML to reduce token count for OpenAI.
    /// Removes navigation, ads, comments, and other non-recipe content.
    /// </summary>
    private string ExtractRecipeContentFromHtml(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Remove unwanted elements: nav, footer, ads, scripts, styles, comments
        var nodesToRemove = doc.DocumentNode.SelectNodes(
            "//script | //style | //nav | //footer | //header | //noscript | //iframe | " +
            "//*[@class='ad' or @class='advertisement' or @class='comments' or @class='sidebar' or contains(@class, 'social')] | " +
            "//*[@id='ad' or @id='ads' or @id='advertisement']");

        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        // Try to find recipe-specific containers
        var recipeContainer = doc.DocumentNode.SelectSingleNode(
            "//*[@class='recipe' or @class='recipe-content' or contains(@class, 'recipe')] | " +
            "//*[@itemtype='https://schema.org/Recipe'] | " +
            "//article") ?? doc.DocumentNode;

        // Extract image URLs before converting to text
        var imageUrls = new List<string>();
        var imgNodes = recipeContainer.SelectNodes(".//img[@src]");
        if (imgNodes != null)
        {
            foreach (var img in imgNodes)
            {
                var src = img.GetAttributeValue("src", "");
                if (!string.IsNullOrWhiteSpace(src) &&
                    Uri.TryCreate(src, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "http" || uri.Scheme == "https"))
                {
                    imageUrls.Add(src);
                }
            }
        }

        // Extract text content
        var text = recipeContainer.InnerText;

        // Clean whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        text = text.Trim();

        // Append image URLs so OpenAI can include them in the response
        if (imageUrls.Count > 0)
        {
            text += "\n\n[RECIPE IMAGES]\n" + string.Join("\n", imageUrls.Distinct());
        }

        // Limit to first 8000 characters to ensure we stay under token limit
        if (text.Length > 8000)
            text = text.Substring(0, 8000) + "\n[Content truncated for token limit]";

        return text;
    }
    private async Task<Guid> GetOrCreateHierarchicalCategoryAsync(string? mainCategoryName, string? subCategoryName)
    {
        // Default values - ensure we never have empty/null values
        mainCategoryName = SanitizeCategoryName(mainCategoryName) ?? "Dinner";
        subCategoryName = SanitizeCategoryName(subCategoryName) ?? "Main Course";

        // Ensure main category name is always valid
        if (string.IsNullOrWhiteSpace(mainCategoryName) || mainCategoryName == "N/A")
            mainCategoryName = "Dinner";

        // Get or create main category (ParentCategoryId = null)
        var mainCategory = await _categoryRepository.GetCategoryByNameAsync(mainCategoryName);
        if (mainCategory == null || mainCategory.ParentCategoryId != null)
        {
            // If it exists but is not a main category (has a parent), create a new one
            mainCategory = new Category { Name = mainCategoryName, ParentCategoryId = null };
            await _categoryRepository.AddCategoryAsync(mainCategory);
            _logger.LogInformation("Created new main category: {CategoryName} with ID: {CategoryId}", mainCategoryName, mainCategory.Id);
        }
        else
        {
            _logger.LogInformation("Found existing main category: {CategoryName}", mainCategoryName);
        }

        // Get or create sub category under the main category
        // First, find if subcategory exists with this parent
        var subCategory = await _categoryRepository.GetCategoryByNameAndParentAsync(subCategoryName, mainCategory.Id);
        
        if (subCategory == null)
        {
            subCategory = new Category 
            { 
                Name = subCategoryName, 
                ParentCategoryId = mainCategory.Id 
            };
            await _categoryRepository.AddCategoryAsync(subCategory);
            _logger.LogInformation("Created new sub category: {SubCategoryName} under {MainCategoryName} with ID: {CategoryId}", 
                subCategoryName, mainCategoryName, subCategory.Id);
        }
        else
        {
            _logger.LogInformation("Found existing sub category: {SubCategoryName} under {MainCategoryName}", subCategoryName, mainCategoryName);
        }

        return subCategory.Id;
    }

    /// <summary>
    /// Sanitizes category name by trimming whitespace and replacing invalid values.
    /// </summary>
    private string? SanitizeCategoryName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        name = name.Trim().Replace('"', ' ').Replace('\'', ' '  );

        // Replace common invalid/generic values
        if (name.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("None", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Other", StringComparison.OrdinalIgnoreCase))
            return null;

        return string.IsNullOrWhiteSpace(name) ? null : name;
    }
}
