using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Interfaces;

public interface IRecipeServices
{
    Task<RecipesDto?> GetRecipeByIdAsync(Guid id);
    Task<IEnumerable<RecipesDto>> GetAllRecipesAsync();
    Task CreateRecipeAsync(RecipesDto recipeDto);
    Task UpdateRecipeAsync(Guid id, RecipesDto recipeDto);
    Task DeleteRecipeAsync(Guid id);


}