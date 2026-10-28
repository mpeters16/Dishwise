namespace RecipeManager.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Email { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string Role { get; set; } = "User";
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}