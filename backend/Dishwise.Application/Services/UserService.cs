using AutoMapper;
using Dishwise.Application.Interfaces;
using Dishwise.Application.Dto;
using System.Security.Cryptography;
using System.Text;

namespace Dishwise.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }


    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);

        if (user == null)
        {
            return null;
        }
        return _mapper.Map<UserDto>(user);

    }

      public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsername(username);

        if (user == null)
        {
            return null;
        }
        return _mapper.Map<UserDto>(user);

    }

  public async Task<UserDto?> ValidateCredentialsAsync(string username, string password)
{
    var user = await GetByUsernameAsync(username);
    if (user == null) return null;

    // BCrypt verify
    bool valid = BCrypt.Net.BCrypt.Verify(password, user.Password);
    return valid ? user : null;
}

public async Task<UserDto> CreateUserAsync(string name, string username, string email, string password)
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        DisplayName = name,
        Username= username,
        Email = email,
        Password = BCrypt.Net.BCrypt.HashPassword(password)
    };

    await _userRepository.AddUser(user);

    return _mapper.Map<UserDto>(user);
}
public async Task<bool> ResetPasswordAsync(string username, string newPassword)
{
    var user = await _userRepository.GetUserByUsername(username);
    if (user == null)
        return false;

    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

    await _userRepository.UpdateUser(user);
    return true;
}


}