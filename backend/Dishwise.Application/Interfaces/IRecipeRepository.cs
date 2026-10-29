namespace Dishwise.Application.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetRecipeByIdAsync(Guid id);
    Task<IEnumerable<Recipe>> GetAllRecipesAsync();
    Task<IEnumerable<Recipe>> GetRecipesWithSourceUrlAsync();
    Task AddRecipeAsync(Recipe recipe);
    Task UpdateRecipeAsync(Recipe r);
    Task DeleteRecipeAsync(Guid id);
}