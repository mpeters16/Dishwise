namespace RecipeManager.Domain.Entities;

public class Instruction : BaseEntity
{
    public string Step { get; set; } = "";
    public int Order { get; set; }
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}