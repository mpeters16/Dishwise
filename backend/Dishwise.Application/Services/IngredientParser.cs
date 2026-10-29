using System.Globalization;
using System.Text.RegularExpressions;
using Dishwise.Application.Dto;

namespace Dishwise.Application.Services;

/// <summary>
/// Splits a free-text ingredient line such as "2 cups (250g) plain flour, sifted (Note 1)"
/// into a quantity, a unit and a name.
/// </summary>
public static class IngredientParser
{
    private static readonly Dictionary<string, double> UnicodeFractions = new()
    {
        ["½"] = 0.5, ["¼"] = 0.25, ["¾"] = 0.75,
        ["⅓"] = 1.0 / 3.0, ["⅔"] = 2.0 / 3.0,
        ["⅛"] = 0.125, ["⅜"] = 0.375, ["⅝"] = 0.625, ["⅞"] = 0.875,
    };

    private static readonly Regex NoteRegex = new(
        @"\(\s*,?\s*Note\s*\d+\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // "(250g)" or "(400 ml)" directly at the start: the quantity and unit in brackets
    private static readonly Regex BracketQuantityRegex = new(
        @"^\((?<qty>\d+(?:\.\d+)?)\s*(?<unit>g|kg|ml|l|oz)\)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // A second measure after a slash, such as "/ 1/2 cup", "/ 7 oz" or "/500g"
    private static readonly Regex AlternateMeasureRegex = new(
        @"^/\s*\d+(?:\.\d+)?(?:\s*\d+/\d+|/\d+)?\s*(?:g|kg|ml|oz|lbs?|cups?|tbsp|tsp|tablespoons?|teaspoons?)\b\.?\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // "1 1/2", "3/4", "2", "2.5", "2-3" (a range keeps its lower bound), or a unicode fraction
    private static readonly Regex QuantityRegex = new(
        @"^(?<qty>\d+\s+\d+/\d+|\d+/\d+|\d+(?:\.\d+)?|[½¼¾⅓⅔⅛⅜⅝⅞])(?:\s*[-–]\s*(?:\d+\s+\d+/\d+|\d+/\d+|\d+(?:\.\d+)?))?\s*",
        RegexOptions.Compiled);

    // Whole-word units only, so "Large" is not read as "L"
    private static readonly Regex UnitRegex = new(
        @"^(?<unit>tablespoons?|tbsps?|tbs|teaspoons?|tsps?|cups?|ounces?|oz|pounds?|lbs?|grams?|g|kilograms?|kg|millilit(?:re|er)s?|ml|litres?|liters?|l|cloves?|pinch(?:es)?|bunch(?:es)?|handfuls?|pieces?|slices?|cans?|packets?|sprigs?)\b\.?\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Parses one ingredient line. Quantity is 0 when the line has none, so the UI shows only the name.
    /// </summary>
    public static (double Quantity, string? Unit, string Name) Parse(string text)
    {
        var remaining = NoteRegex.Replace(text, " ").Trim().TrimStart('/', ' ');
        remaining = Regex.Replace(remaining, @"\s+", " ");

        double quantity = 0;
        string? unit = null;

        var bracket = BracketQuantityRegex.Match(remaining);
        if (bracket.Success)
        {
            quantity = ParseNumber(bracket.Groups["qty"].Value);
            unit = bracket.Groups["unit"].Value;
            remaining = remaining[bracket.Length..];
        }
        else
        {
            var qty = QuantityRegex.Match(remaining);
            if (qty.Success)
            {
                quantity = ParseQuantity(qty.Groups["qty"].Value);
                remaining = remaining[qty.Length..];
            }

            var unitMatch = UnitRegex.Match(remaining);
            if (unitMatch.Success && quantity > 0)
            {
                unit = unitMatch.Groups["unit"].Value;
                remaining = remaining[unitMatch.Length..];

                // Recipe sites often list a second measure: "115g / 1/2 cup butter". Keep the first.
                remaining = AlternateMeasureRegex.Replace(remaining, "", 1);
            }
        }

        if (remaining.StartsWith("of ", StringComparison.OrdinalIgnoreCase))
            remaining = remaining[3..];

        return (quantity, unit, CleanName(remaining, text));
    }

    /// <summary>
    /// Removes duplicate ingredients (same name, quantity and unit), keeping the first occurrence.
    /// </summary>
    public static List<IngredientsDto> Deduplicate(IEnumerable<IngredientsDto> ingredients)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<IngredientsDto>();

        foreach (var ingredient in ingredients)
        {
            var key = $"{ingredient.Name.Trim()}|{ingredient.Quantity}|{ingredient.Unit?.Trim()}";
            if (seen.Add(key)) result.Add(ingredient);
        }

        return result;
    }

    private static string CleanName(string remaining, string original)
    {
        // A leading bracket such as "(400g)" is a quantity note, not part of the name
        remaining = Regex.Replace(remaining, @"^\([^)]*\)\s*", "");

        // The name is what comes before any bracketed or comma-separated preparation notes
        var cut = remaining.IndexOfAny(['(', ',']);
        var name = (cut >= 0 ? remaining[..cut] : remaining).Trim().TrimEnd('/', ' ');

        if (name.Length == 0)
            name = Regex.Replace(original, @"[()]", " ").Trim();

        return Regex.Replace(name, @"\s+", " ");
    }

    private static double ParseQuantity(string value)
    {
        if (UnicodeFractions.TryGetValue(value, out var fraction)) return fraction;

        // "1 1/2" is a whole number plus a fraction
        var mixed = Regex.Match(value, @"^(\d+)\s+(\d+)/(\d+)$");
        if (mixed.Success)
        {
            var den = ParseNumber(mixed.Groups[3].Value);
            return den > 0 ? ParseNumber(mixed.Groups[1].Value) + ParseNumber(mixed.Groups[2].Value) / den : 0;
        }

        if (value.Contains('/'))
        {
            var parts = value.Split('/');
            var den = ParseNumber(parts[1]);
            return den > 0 ? ParseNumber(parts[0]) / den : 0;
        }

        return ParseNumber(value);
    }

    private static double ParseNumber(string value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : 0;
}
