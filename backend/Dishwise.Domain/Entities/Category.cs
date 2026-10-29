namespace Dishwise.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Parent category ID for hierarchical structure. Null if this is a main category.
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
    
    /// <summary>
    /// Navigation property for parent category.
    /// </summary>
    public Category? ParentCategory { get; set; }
    
    /// <summary>
    /// Navigation property for child/subcategories.
    /// </summary>
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}