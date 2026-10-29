using Microsoft.EntityFrameworkCore;

namespace Dishwise.Infrastructure.Data;

public class RecipeRepository : IRecipeRepository
{
    private readonly ApplicationDbContext _db;

    public RecipeRepository(ApplicationDbContext context)
    {
        _db = context;
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
    {
        return await _db.Recipes.Include(r => r.Ingredients)
                                .Include(r => r.Instructions)
                                .Include(r => r.Category)
                                .Include(r => r.Images)
                                .Include(r => r.Nutrition)
                                .ToListAsync();
    }

    public async Task<IEnumerable<Recipe>> GetRecipesWithSourceUrlAsync()
    {
        return await _db.Recipes
            .Where(r => r.SourceUrl != null && r.SourceUrl != "")
            .Select(r => new Recipe { Id = r.Id, Title = r.Title, SourceUrl = r.SourceUrl })
            .ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
    {
        return await _db.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .Include(r => r.Category)
            .Include(r => r.Images)
            .Include(r => r.Nutrition)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Recipe?> GetRecipeByIdForUserAsync(Guid id, Guid userId)
    {
        var recipe = await _db.Recipes.FindAsync(id);
        if (recipe == null || recipe.UserId != userId)
        {
            return null;
        }
        return recipe;
    }
    public async Task AddRecipeAsync(Recipe recipe)
    {
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRecipeAsync(Recipe r)
    {
        var recipe = await _db.Recipes.FindAsync(r.Id);
        if (recipe == null)
        {
            throw new KeyNotFoundException("Recipe not found.");
        }

        recipe.Title = r.Title;
        recipe.Description = r.Description;
        recipe.PrepTimeMinutes = r.PrepTimeMinutes;
        recipe.CookTimeMinutes = r.CookTimeMinutes;
        recipe.CategoryId = r.CategoryId;
        recipe.Ingredients = r.Ingredients;
        recipe.Instructions = r.Instructions;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(Guid id)
    {
        var recipe = await _db.Recipes.FindAsync(id);
        if (recipe == null)
        {
            throw new KeyNotFoundException("Recipe not found.");
        }

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task GetRecipesByUserIdAsync(Guid userId)
    {
        var recipes = await _db.Recipes.Where(r => r.UserId == userId).ToListAsync();
    }

    public async Task<Recipe?> SetImageUrlAsync(Guid id, Guid userId, string url)
    {
        var recipe = await _db.Recipes.FindAsync(id);
        if (recipe == null || recipe.UserId != userId)
        {
            return null;
        }

        // recipe.ImageId = Guid.NewGuid();
        recipe.Images = new List<RecipeImage>
        {
            new RecipeImage
            {
                Url = url
            }
        };
        await _db.SaveChangesAsync();

        return recipe;
    }
}