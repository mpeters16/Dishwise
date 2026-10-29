namespace Dishwise.Domain.Entities;

public class Ingredient : BaseEntity
{
    public string Name { get; set; } = "";
    public double Quantity { get; set; } = 0;
    public string? Unit { get; set; } = "";
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}