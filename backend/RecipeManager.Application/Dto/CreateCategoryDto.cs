namespace RecipeManager.Application.Dto;

public class CreateCategoryDto
{
    public string Name { get; set; } = "";
    public Guid? ParentCategoryId { get; set; }
}
