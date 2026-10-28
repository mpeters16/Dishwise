using Microsoft.EntityFrameworkCore;


namespace RecipeManager.Infrastructure.Data;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _db.Categories
            .Include(c => c.SubCategories)
            .ToListAsync();
    }
    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _db.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<Category?> GetCategoryByNameAndParentAsync(string name, Guid parentId)
    {
        return await _db.Categories.FirstOrDefaultAsync(c => 
            c.Name.ToLower() == name.ToLower() && 
            c.ParentCategoryId == parentId);
    }

    public async Task AddCategoryAsync(Category category)
    {
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        var existingCategory = await _db.Categories.FindAsync(category.Id);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        existingCategory.Name = category.Name;
        existingCategory.ParentCategoryId = category.ParentCategoryId;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category != null)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> HasRecipesAsync(Guid id)
    {
        return await _db.Recipes.AnyAsync(r => r.CategoryId == id);
    }
}