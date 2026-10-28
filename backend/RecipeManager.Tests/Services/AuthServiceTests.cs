using Microsoft.Extensions.Configuration;
using RecipeManager.Application.Services;
using RecipeManager.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace RecipeManager.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var initialData = new Dictionary<string, string?>
            {
                {"Jwt:Key", "iziBA0hO2zuz5S8vlBHxxuP9pC/NXnfj5GCsZtGPHZYFuXQNfEn0H/vlrvdGewSb"},
                {"Jwt:Issuer", "RecipeManagerApi"},
                {"Jwt:Audience", "RecipeManagerApiUsers"},
                {"Jwt:ExpireMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(initialData)
               .Build();

            _authService = new AuthService(configuration);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            //arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test User",
                Username = "testuser",
                Role = "User"
            };

            //act
            var token = _authService.GenerateToken(user.Id, user.DisplayName, user.Username, user.Role);

            //assert
            Assert.NotNull(token);
            Assert.True(token.Split('.').Length == 3, "Token should have three parts separated by dots");
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            //arrange
            var password = "MySecurePassword!";

            //act
            var hashedPassword = _authService.HashPassword(password);

            //assert
            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
        {
            //arrange
            var password = "MySecurePassword!";
            var hashedPassword = _authService.HashPassword(password);

            //act
            var isValid = _authService.VerifyPassword(password, hashedPassword);

            //assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
        {
            //arrange
            var correctPassword = "MySecurePassword!";
            var incorrectPassword = "WrongPassword!";
            var hashedPassword = _authService.HashPassword(correctPassword);

            //act
            var isValid = _authService.VerifyPassword(incorrectPassword, hashedPassword);

            //assert
            Assert.False(isValid);
        }
    }
}