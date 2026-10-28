using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Interfaces;

public interface IRecipesImportService
{
    Task<RecipesDto> ImportRecipeFromUrlAsync(string url, Guid userId, CancellationToken cancellationToken = default);
    Task<RecipesDto> ImportRecipeFromTextAsync(string rawText, Guid userId, CancellationToken cancellationToken = default);
}