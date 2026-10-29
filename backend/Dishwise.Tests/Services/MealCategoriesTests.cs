using Dishwise.Application.Services;
using Xunit;

namespace Dishwise.Tests.Services
{
    public class MealCategoriesTests
    {
        [Theory]
        [InlineData("Dinner", "Dinner")]
        [InlineData("dessert", "Dessert")]
        [InlineData("  Breakfast  ", "Breakfast")]
        [InlineData("Main course", "Dinner")]
        [InlineData("Side dish", "Dinner")]
        [InlineData("Drinks", "Beverage")]
        [InlineData("Cake recipes", "Baking")]
        [InlineData("Brunch", "Breakfast")]
        public void NormalizeMain_MapsKnownLabels(string raw, string expected)
        {
            Assert.Equal(expected, MealCategories.NormalizeMain(raw));
        }

        [Theory]
        [InlineData("Western")]
        [InlineData("Italian")]
        [InlineData("N/A")]
        [InlineData("")]
        [InlineData(null)]
        public void NormalizeMain_ReturnsNullForUnknownLabels(string? raw)
        {
            Assert.Null(MealCategories.NormalizeMain(raw));
        }

        [Theory]
        [InlineData("Steak Fajitas", null)]
        [InlineData("Breadcrumb Chicken", null)]
        [InlineData("Birthday Cake", "Baking")]
        [InlineData("Strawberry Smoothie", "Beverage")]
        [InlineData("Blueberry Pancakes", "Breakfast")]
        [InlineData("Vanilla Ice Cream", "Dessert")]
        [InlineData("Garlic Hummus", "Snack")]
        public void InferMainFromTitle_MatchesWholeWordsOnly(string title, string? expected)
        {
            Assert.Equal(expected, MealCategories.InferMainFromTitle(title));
        }

        [Theory]
        [InlineData("Chicken", "Chicken")]
        [InlineData("\"Thai\"", "Thai")]
        [InlineData("Pasta,  Italian", "Pasta Italian")]
        [InlineData("Dinner", null)]
        [InlineData("N/A", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void CleanSub_StripsJunkAndRejectsMainNames(string? raw, string? expected)
        {
            Assert.Equal(expected, MealCategories.CleanSub(raw));
        }

        [Fact]
        public void CleanSub_TruncatesLongLabels()
        {
            var longLabel = new string('a', 80);

            var cleaned = MealCategories.CleanSub(longLabel);

            Assert.NotNull(cleaned);
            Assert.True(cleaned!.Length <= 50);
        }
    }
}
