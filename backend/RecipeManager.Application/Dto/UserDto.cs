namespace RecipeManager.Application.Dto;

public class UserDto
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public string DisplayName { get; set; } = "";
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User";
}