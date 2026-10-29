namespace Dishwise.Application.Interfaces;

public interface IAuthService
{
    string GenerateToken(Guid userId,string name, string username, string role);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
