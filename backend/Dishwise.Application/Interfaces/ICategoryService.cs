using Dishwise.Application.Dto;

namespace Dishwise.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task AddCategoryAsync(CategoryDto category);
    Task UpdateCategoryAsync(Guid id, CategoryDto category);
    Task DeleteCategoryAsync(Guid id);
}