using Dishwise.Application.Services;
using Xunit;

namespace Dishwise.Tests.Services
{
    public class RecipeSourceUrlTests
    {
        [Theory]
        [InlineData("https://www.recipetineats.com/birthday-cake/", "recipetineats.com/birthday-cake")]
        [InlineData("http://recipetineats.com/birthday-cake", "recipetineats.com/birthday-cake")]
        [InlineData("https://RecipeTinEats.com/Birthday-Cake/?utm_source=x#top", "recipetineats.com/birthday-cake")]
        [InlineData("  https://www.recipetineats.com/birthday-cake/  ", "recipetineats.com/birthday-cake")]
        public void Normalize_IdentifiesTheSamePage(string url, string expected)
        {
            Assert.Equal(expected, RecipeSourceUrl.Normalize(url));
        }

        [Fact]
        public void Normalize_KeepsDifferentPagesApart()
        {
            Assert.NotEqual(
                RecipeSourceUrl.Normalize("https://www.recipetineats.com/birthday-cake/"),
                RecipeSourceUrl.Normalize("https://www.recipetineats.com/birthday-cake-2/"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_EmptyInputGivesEmptyKey(string? url)
        {
            Assert.Equal("", RecipeSourceUrl.Normalize(url));
        }
    }
}
