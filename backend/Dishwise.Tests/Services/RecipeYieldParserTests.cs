using Dishwise.Application.Services;
using Xunit;

namespace Dishwise.Tests.Services
{
    public class RecipeYieldParserTests
    {
        [Theory]
        [InlineData("4 servings", 4)]
        [InlineData("Serves 6", 6)]
        [InlineData("Makes 12 muffins", 12)]
        [InlineData("4", 4)]
        [InlineData("  2 portions ", 2)]
        public void Parse_ReadsTheFirstNumber(string yield, int expected)
        {
            Assert.Equal(expected, RecipeYieldParser.Parse(yield));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a few")]
        [InlineData("0 servings")]
        [InlineData("500 servings")]
        public void Parse_ReturnsNullWhenThereIsNoUsableNumber(string? yield)
        {
            Assert.Null(RecipeYieldParser.Parse(yield));
        }
    }
}
