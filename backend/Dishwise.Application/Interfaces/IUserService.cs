using Dishwise.Application.Dto;

namespace Dishwise.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto?> ValidateCredentialsAsync( string email, string password);
        Task<UserDto> CreateUserAsync(string name, string username, string email, string password);
        Task<bool> ResetPasswordAsync(string username, string newPassword);
    }
}
