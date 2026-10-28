using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Interfaces;

public interface IRecipesUrlImportService
{
    Task<RecipesDto> ImportFromUrlAsync(
       string url,
       Guid userId,
       Guid categoryId,
       CancellationToken cancellationToken = default);
}