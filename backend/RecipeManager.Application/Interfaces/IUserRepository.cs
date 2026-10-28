namespace RecipeManager.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserById(Guid id);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByUsername(string username);
    Task AddUser(User user);
    Task UpdateUser(User user);
    Task DeleteUser(User user);
}