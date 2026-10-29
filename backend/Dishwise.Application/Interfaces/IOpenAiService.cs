using Dishwise.Application.Dto;

namespace Dishwise.Application.Interfaces;

public interface IOpenAiService
{
    Task<RecipesDto> ParseRecipeAsync(
       string rawText,
       CancellationToken cancellationToken = default);

    Task<RecipesDto> ParseRecipeFromHtmlAsync(
        string html,
        CancellationToken cancellationToken = default);
}