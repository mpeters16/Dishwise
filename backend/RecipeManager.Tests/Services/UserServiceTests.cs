using AutoMapper;
using Moq;
using RecipeManager.Application.Dto;
using RecipeManager.Application.Interfaces;
using RecipeManager.Application.Services;
using RecipeManager.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RecipeManager.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
            _userService = new UserService(_mockUserRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var user = new User { Email = "test@test.com" };
            var userDto = new UserDto { Email = "test@test.com" };
            _mockUserRepo.Setup(repo => repo.GetUserByEmail("test@test.com")).ReturnsAsync(user);
            _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetByEmailAsync("test@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByEmailAsync("test@test.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var user = new User { Username = "testuser" };
            var userDto = new UserDto { Username = "testuser" };
            _mockUserRepo.Setup(repo => repo.GetUserByUsername("testuser")).ReturnsAsync(user);
            _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ShouldReturnUserDto_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "password";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = "testuser", Password = hashedPassword };
            var userDto = new UserDto { Username = "testuser", Password = hashedPassword };
            _mockUserRepo.Setup(repo => repo.GetUserByUsername("testuser")).ReturnsAsync(user);
            _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.ValidateCredentialsAsync("testuser", password);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ShouldReturnNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var password = "password";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = "testuser", Password = hashedPassword };
            var userDto = new UserDto { Username = "testuser", Password = hashedPassword };
            _mockUserRepo.Setup(repo => repo.GetUserByUsername("testuser")).ReturnsAsync(user);
            _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.ValidateCredentialsAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnUserDto()
        {
            // Arrange
            var userDto = new UserDto();
            _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

            // Act
            var result = await _userService.CreateUserAsync("Test", "testuser", "test@test.com", "password");

            // Assert
            _mockUserRepo.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var user = new User { Username = "testuser" };
            _mockUserRepo.Setup(repo => repo.GetUserByUsername("testuser")).ReturnsAsync(user);

            // Act
            var result = await _userService.ResetPasswordAsync("testuser", "newpassword");

            // Assert
            Assert.True(result);
            _mockUserRepo.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetUserByUsername(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.ResetPasswordAsync("testuser", "newpassword");

            // Assert
            Assert.False(result);
        }
    }
}
