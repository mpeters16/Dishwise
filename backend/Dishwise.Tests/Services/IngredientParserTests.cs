using Dishwise.Application.Dto;
using Dishwise.Application.Services;
using Xunit;

namespace Dishwise.Tests.Services
{
    public class IngredientParserTests
    {
        [Theory]
        [InlineData("2 cups flour", 2, "cups", "flour")]
        [InlineData("1/2 tsp salt", 0.5, "tsp", "salt")]
        [InlineData("1 1/2 cups milk", 1.5, "cups", "milk")]
        [InlineData("½ tsp vanilla", 0.5, "tsp", "vanilla")]
        [InlineData("2-3 cloves garlic, crushed", 2, "cloves", "garlic")]
        [InlineData("1 tbsp olive oil", 1, "tbsp", "olive oil")]
        [InlineData("4 large eggs", 4, null, "large eggs")]
        [InlineData("1 onion", 1, null, "onion")]
        [InlineData("3 large carrots, chopped", 3, null, "large carrots")]
        public void Parse_ExtractsQuantityUnitAndName(string text, double quantity, string? unit, string name)
        {
            var (q, u, n) = IngredientParser.Parse(text);

            Assert.Equal(quantity, q, 3);
            Assert.Equal(unit, u);
            Assert.Equal(name, n);
        }

        [Fact]
        public void Parse_DoesNotReadLargeAsLitres()
        {
            var (quantity, unit, name) = IngredientParser.Parse("Large eggs");

            Assert.Equal(0, quantity);
            Assert.Null(unit);
            Assert.Equal("Large eggs", name);
        }

        [Fact]
        public void Parse_UsesLeadingBracketAsQuantityAndUnit()
        {
            var (quantity, unit, name) = IngredientParser.Parse("(250g)  soft icing sugar / powdered sugar (, sifted (Note 9))");

            Assert.Equal(250, quantity);
            Assert.Equal("g", unit);
            Assert.Equal("soft icing sugar / powdered sugar", name);
        }

        [Fact]
        public void Parse_RemovesLeadingSlashAndPreparationNotes()
        {
            var (quantity, unit, name) = IngredientParser.Parse("/ 1/2 cup  unsalted butter (, cut into 1.5cm cubes)");

            Assert.Equal(0.5, quantity, 3);
            Assert.Equal("cup", unit);
            Assert.Equal("unsalted butter", name);
        }

        [Fact]
        public void Parse_IgnoresBracketsAfterUnit()
        {
            var (quantity, unit, name) = IngredientParser.Parse("1 can (400g) chickpeas");

            Assert.Equal(1, quantity);
            Assert.Equal("can", unit);
            Assert.Equal("chickpeas", name);
        }

        [Theory]
        [InlineData("115g / 1/2 cup  unsalted butter (, cut into cubes)", 115, "g", "unsalted butter")]
        [InlineData("200g / 7 oz  dark chocolate chips", 200, "g", "dark chocolate chips")]
        [InlineData("1.2kg /2.5 lb  chuck beef", 1.2, "kg", "chuck beef")]
        public void Parse_KeepsOnlyTheFirstMeasure(string text, double quantity, string unit, string name)
        {
            var (q, u, n) = IngredientParser.Parse(text);

            Assert.Equal(quantity, q, 3);
            Assert.Equal(unit, u);
            Assert.Equal(name, n);
        }

        [Fact]
        public void Parse_LineWithoutQuantityHasZeroQuantity()
        {
            var (quantity, unit, name) = IngredientParser.Parse("salt and pepper");

            Assert.Equal(0, quantity);
            Assert.Null(unit);
            Assert.Equal("salt and pepper", name);
        }

        [Fact]
        public void Deduplicate_RemovesRepeatedIngredientsAndKeepsOrder()
        {
            var input = new List<IngredientsDto>
            {
                new() { Name = "flour", Quantity = 2, Unit = "cups" },
                new() { Name = "Flour", Quantity = 2, Unit = "cups" },
                new() { Name = "flour", Quantity = 1, Unit = "cups" },
                new() { Name = "eggs", Quantity = 2, Unit = null },
            };

            var result = IngredientParser.Deduplicate(input);

            Assert.Equal(3, result.Count);
            Assert.Equal("flour", result[0].Name);
            Assert.Equal(2, result[0].Quantity);
            Assert.Equal(1, result[1].Quantity);
            Assert.Equal("eggs", result[2].Name);
        }
    }
}
