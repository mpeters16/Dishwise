using Dishwise.Application.Dto;

namespace Dishwise.Application.Interfaces;

public interface IRecipesUrlImportService
{
    Task<RecipesDto> ImportFromUrlAsync(
       string url,
       Guid userId,
       Guid categoryId,
       CancellationToken cancellationToken = default);
}