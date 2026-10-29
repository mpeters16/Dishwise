using System.Text.RegularExpressions;

namespace Dishwise.Application.Services;

/// <summary>
/// Reads the number of servings from a schema.org recipeYield value such as
/// "4 servings", "Serves 4", "Makes 12 muffins" or "4".
/// </summary>
public static class RecipeYieldParser
{
    private static readonly Regex FirstNumber = new(@"\d+", RegexOptions.Compiled);

    public static int? Parse(string? recipeYield)
    {
        if (string.IsNullOrWhiteSpace(recipeYield)) return null;

        var match = FirstNumber.Match(recipeYield);
        if (!match.Success || !int.TryParse(match.Value, out var servings)) return null;

        return servings is >= 1 and <= 100 ? servings : null;
    }
}
