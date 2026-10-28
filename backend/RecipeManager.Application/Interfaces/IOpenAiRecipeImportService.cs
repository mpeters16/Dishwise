using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Interfaces;

public interface IOpenAiRecipeImportService
{
    /// <summary>
    /// Imports a recipe from a URL using OpenAI to parse and enhance the recipe content.
    /// </summary>
    /// <param name="url">The URL of the webpage containing the recipe</param>
    /// <param name="userId">The ID of the user importing the recipe</param>
    /// <param name="categoryId">The ID of the category for the recipe</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed and saved recipe</returns>
    Task<RecipesDto> ImportRecipeFromUrlWithOpenAiAsync(
        string url,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports and enhances a recipe from raw text using OpenAI.
    /// </summary>
    /// <param name="rawRecipeText">The raw recipe text to parse</param>
    /// <param name="userId">The ID of the user importing the recipe</param>
    /// <param name="categoryId">The ID of the category for the recipe</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed and saved recipe</returns>
    Task<RecipesDto> ImportRecipeFromTextWithOpenAiAsync(
        string rawRecipeText,
        Guid userId,
        CancellationToken cancellationToken = default);
}
