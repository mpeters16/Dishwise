using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Interfaces;

public interface IOpenAiService
{
    Task<RecipesDto> ParseRecipeAsync(
       string rawText,
       CancellationToken cancellationToken = default);

    Task<RecipesDto> ParseRecipeFromHtmlAsync(
        string html,
        CancellationToken cancellationToken = default);
}