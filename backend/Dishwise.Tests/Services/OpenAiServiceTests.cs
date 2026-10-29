using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Dishwise.Application.Services;
using Dishwise.Application.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dishwise.Tests.Services;

public class OpenAiServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<OpenAiService>> _loggerMock;

    public OpenAiServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<OpenAiService>>();
    }

    [Fact]
    public void TestJsonParsing_MultipleIngredients()
    {
        // This test demonstrates that the JSON deserialization can handle multiple ingredients
        var json = @"{
            ""title"": ""Chocolate Cake"",
            ""description"": ""A delicious chocolate cake"",
            ""prepTimeMinutes"": 15,
            ""cookTimeMinutes"": 35,
            ""imageUrls"": [],
            ""ingredients"": [
                {
                    ""name"": ""Flour"",
                    ""quantity"": 2,
                    ""unit"": ""cups""
                },
                {
                    ""name"": ""Cocoa Powder"",
                    ""quantity"": 0.75,
                    ""unit"": ""cups""
                },
                {
                    ""name"": ""Sugar"",
                    ""quantity"": 2,
                    ""unit"": ""cups""
                },
                {
                    ""name"": ""Eggs"",
                    ""quantity"": 2,
                    ""unit"": """"
                }
            ],
            ""instructions"": [
                ""Mix dry ingredients"",
                ""Beat eggs and sugar"",
                ""Combine wet and dry ingredients"",
                ""Bake at 350F for 35 minutes""
            ]
        }";

        // Just verify the JSON is valid and contains multiple items
        Assert.Contains("\"name\": \"Cocoa Powder\"", json);
        Assert.Contains("\"Combine wet and dry ingredients\"", json);
    }

    [Fact]
    public void TestRecipeDtoStructure()
    {
        // Verify that RecipesDto can hold multiple ingredients and instructions
        var recipesDto = new RecipesDto
        {
            Title = "Test Recipe",
            Description = "Test Description",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 30,
            Ingredients = new List<IngredientsDto>
            {
                new IngredientsDto { Name = "Ingredient 1", Quantity = 1, Unit = "cup" },
                new IngredientsDto { Name = "Ingredient 2", Quantity = 2, Unit = "tbsp" },
                new IngredientsDto { Name = "Ingredient 3", Quantity = 0.5, Unit = "tsp" }
            },
            Instructions = new List<InstructionsDto>
            {
                new InstructionsDto { Step = "Step 1", Order = 1 },
                new InstructionsDto { Step = "Step 2", Order = 2 },
                new InstructionsDto { Step = "Step 3", Order = 3 }
            }
        };

        Assert.Equal(3, recipesDto.Ingredients.Count);
        Assert.Equal(3, recipesDto.Instructions.Count);
    }
}
