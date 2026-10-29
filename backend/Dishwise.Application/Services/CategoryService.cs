using AutoMapper;
using Dishwise.Application.Interfaces;
using Dishwise.Application.Dto;

namespace Dishwise.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task AddCategoryAsync(CategoryDto category)
        {
            await _categoryRepository.AddCategoryAsync(_mapper.Map<Category>(category));
        }

        public async Task UpdateCategoryAsync(Guid id, CategoryDto category)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            _mapper.Map(category, existingCategory);
            await _categoryRepository.UpdateCategoryAsync(existingCategory);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException("Category not found");

            if (await _categoryRepository.HasRecipesAsync(id))
                throw new InvalidOperationException("Cannot delete a category that contains recipes. Reassign or delete the recipes first.");

            await _categoryRepository.DeleteCategoryAsync(id);
        }
    }
}