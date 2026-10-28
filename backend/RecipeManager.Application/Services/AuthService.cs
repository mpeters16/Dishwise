using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using RecipeManager.Application.Interfaces;

namespace RecipeManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public string HashPassword(string password)
    {
        // using var sha256 = SHA256.Create();
        // var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        // return Convert.ToBase64String(bytes);

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // using var sha256 = SHA256.Create();
        // var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        // var hash = Convert.ToBase64String(bytes);
        // return hash == hashedPassword;

        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
    public string GenerateToken(Guid userId, string name, string username, string role)
    {
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),  // Standard JWT "sub" claim for subject (user ID)
            new Claim(ClaimTypes.Name, name),         
            new Claim(ClaimTypes.Role, role),
            new Claim("username", username)            
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key not configured"))
        );

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpireMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}