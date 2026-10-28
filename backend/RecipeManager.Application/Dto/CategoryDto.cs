namespace RecipeManager.Application.Dto;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid? ParentCategoryId { get; set; }
    public ICollection<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
}
