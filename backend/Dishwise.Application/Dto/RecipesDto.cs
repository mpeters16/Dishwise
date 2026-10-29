
namespace Dishwise.Application.Dto;

public class RecipesDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }

    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Main category name inferred by OpenAI parser (e.g., Breakfast, Lunch, Dinner, Dessert, Beverage, Baking).
    /// Used to auto-create/find main category if it doesn't exist.
    /// </summary>
    public string? MainCategoryName { get; set; }
    
    /// <summary>
    /// Sub category name inferred by OpenAI parser (e.g., Chicken, Beef, Pork, Vegetarian for Dinner).
    /// Used to auto-create/find subcategory if it doesn't exist.
    /// </summary>
    public string? SubCategoryName { get; set; }

    public CategoryDto Category { get; set; }=new CategoryDto();

    public ICollection<IngredientsDto> Ingredients { get; set; } = new List<IngredientsDto>();
    public ICollection<InstructionsDto> Instructions { get; set; } = new List<InstructionsDto>();
    public ICollection<RecipeImageDto> Images { get; set; } = new List<RecipeImageDto>();
    public NutritionDto? Nutrition { get; set; }
    public string? SourceUrl { get; set; }
}
