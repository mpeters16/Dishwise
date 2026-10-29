namespace Dishwise.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<Category?> GetMainCategoryByNameAsync(string name);
    Task<Category?> GetCategoryByNameAndParentAsync(string name, Guid parentId);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Guid id);
    Task<bool> HasRecipesAsync(Guid id);
}