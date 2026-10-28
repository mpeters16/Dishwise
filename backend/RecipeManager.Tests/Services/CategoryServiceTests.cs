using AutoMapper;
using Moq;
using RecipeManager.Application.Dto;
using RecipeManager.Application.Interfaces;
using RecipeManager.Application.Services;
using RecipeManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RecipeManager.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockRepo = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new CategoryService(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category> { new Category { Id = Guid.NewGuid(), Name = "Test" } };
            var categoryDtos = new List<CategoryDto> { new CategoryDto { Id = categories[0].Id, Name = "Test" } };
            _mockRepo.Setup(repo => repo.GetAllCategoriesAsync()).ReturnsAsync(categories);
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<CategoryDto>>(categories)).Returns(categoryDtos);

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category { Id = categoryId, Name = "Test" };
            var categoryDto = new CategoryDto { Id = categoryId, Name = "Test" };
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync(category);
            _mockMapper.Setup(mapper => mapper.Map<CategoryDto?>(category)).Returns(categoryDto);

            // Act
            var result = await _service.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _service.GetCategoryByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddCategoryAsync_ShouldCallRepository()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "New Category" };
            var category = new Category { Name = "New Category" };
            _mockMapper.Setup(mapper => mapper.Map<Category>(categoryDto)).Returns(category);
            _mockRepo.Setup(repo => repo.AddCategoryAsync(category)).Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(categoryDto);

            // Assert
            _mockRepo.Verify(repo => repo.AddCategoryAsync(category), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDto = new CategoryDto { Id = categoryId, Name = "Updated" };
            var existingCategory = new Category { Id = categoryId, Name = "Original" };
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync(existingCategory);

            // Act
            await _service.UpdateCategoryAsync(categoryId, categoryDto);

            // Assert
            _mockRepo.Verify(repo => repo.UpdateCategoryAsync(existingCategory), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map(categoryDto, existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDto = new CategoryDto { Id = categoryId, Name = "Updated" };
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateCategoryAsync(categoryId, categoryDto));
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldCallRepository_WhenCategoryHasNoRecipes()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync(new Category { Id = categoryId });
            _mockRepo.Setup(repo => repo.HasRecipesAsync(categoryId)).ReturnsAsync(false);
            _mockRepo.Setup(repo => repo.DeleteCategoryAsync(categoryId)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteCategoryAsync(categoryId);

            // Assert
            _mockRepo.Verify(repo => repo.DeleteCategoryAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowKeyNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteCategoryAsync(categoryId));
            _mockRepo.Verify(repo => repo.DeleteCategoryAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowInvalidOperation_WhenCategoryHasRecipes()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.GetCategoryByIdAsync(categoryId)).ReturnsAsync(new Category { Id = categoryId });
            _mockRepo.Setup(repo => repo.HasRecipesAsync(categoryId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteCategoryAsync(categoryId));
            _mockRepo.Verify(repo => repo.DeleteCategoryAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
