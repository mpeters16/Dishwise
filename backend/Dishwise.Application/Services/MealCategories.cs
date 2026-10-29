using System.Text.RegularExpressions;

namespace Dishwise.Application.Services;

/// <summary>
/// The fixed set of top-level meal categories, plus the rules for mapping labels
/// from recipe sites or the AI parser onto them.
/// </summary>
public static class MealCategories
{
    public const string Breakfast = "Breakfast";
    public const string Lunch = "Lunch";
    public const string Dinner = "Dinner";
    public const string Dessert = "Dessert";
    public const string Beverage = "Beverage";
    public const string Baking = "Baking";
    public const string Snack = "Snack";

    public const string DefaultMain = Dinner;
    public const string DefaultSub = "General";

    public static readonly IReadOnlyList<string> All =
        [Breakfast, Lunch, Dinner, Dessert, Beverage, Baking, Snack];

    // Labels that are not real categories and should be ignored.
    private static readonly HashSet<string> JunkLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "N/A", "None", "Unknown", "Other", "General", "Recipe", "Recipes"
    };

    // Whole-label synonyms (compared case-insensitively after trimming).
    private static readonly Dictionary<string, string> Synonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["breakfast"] = Breakfast,
        ["brunch"] = Breakfast,

        ["lunch"] = Lunch,
        ["sandwich"] = Lunch,
        ["sandwiches"] = Lunch,
        ["salad"] = Lunch,
        ["salads"] = Lunch,
        ["soup"] = Lunch,
        ["soups"] = Lunch,

        ["dinner"] = Dinner,
        ["main"] = Dinner,
        ["mains"] = Dinner,
        ["main course"] = Dinner,
        ["main courses"] = Dinner,
        ["main dish"] = Dinner,
        ["main dishes"] = Dinner,
        ["entree"] = Dinner,
        ["entrees"] = Dinner,
        ["side"] = Dinner,
        ["sides"] = Dinner,
        ["side dish"] = Dinner,
        ["side dishes"] = Dinner,
        ["supper"] = Dinner,

        ["dessert"] = Dessert,
        ["desserts"] = Dessert,
        ["sweet"] = Dessert,
        ["sweets"] = Dessert,
        ["pudding"] = Dessert,
        ["ice cream"] = Dessert,

        ["beverage"] = Beverage,
        ["beverages"] = Beverage,
        ["drink"] = Beverage,
        ["drinks"] = Beverage,
        ["smoothie"] = Beverage,
        ["smoothies"] = Beverage,
        ["cocktail"] = Beverage,
        ["cocktails"] = Beverage,
        ["coffee"] = Beverage,
        ["tea"] = Beverage,
        ["juice"] = Beverage,
        ["lemonade"] = Beverage,

        ["baking"] = Baking,
        ["bake"] = Baking,
        ["bread"] = Baking,
        ["breads"] = Baking,
        ["cake"] = Baking,
        ["cakes"] = Baking,
        ["cookie"] = Baking,
        ["cookies"] = Baking,
        ["muffin"] = Baking,
        ["muffins"] = Baking,
        ["pastry"] = Baking,
        ["pastries"] = Baking,
        ["pie"] = Baking,
        ["pies"] = Baking,
        ["tart"] = Baking,
        ["tarts"] = Baking,

        ["snack"] = Snack,
        ["snacks"] = Snack,
        ["appetizer"] = Snack,
        ["appetizers"] = Snack,
        ["starter"] = Snack,
        ["starters"] = Snack,
        ["dip"] = Snack,
        ["dips"] = Snack,
    };

    // Title keywords, checked in order, matched on whole words only.
    private static readonly (string Category, Regex Pattern)[] TitleRules =
    [
        (Breakfast, new Regex(@"\b(breakfast|pancakes?|waffles?|omelet(te|)s?|porridge|granola|french toast)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        (Beverage, new Regex(@"\b(smoothies?|juice|cocktails?|lemonade|tea|coffee|latte|milkshakes?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        (Baking, new Regex(@"\b(cakes?|cookies?|bread|muffins?|brownies?|scones?|biscuits?|tarts?|pies?|pastry|croissants?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        (Dessert, new Regex(@"\b(puddings?|ice cream|mousse|cheesecake|desserts?|sorbet|custard|trifle|crumble)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        (Snack, new Regex(@"\b(dips?|hummus|guacamole|appetizers?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
    ];

    /// <summary>
    /// Maps a label to one of the top-level categories, or returns null if it isn't recognised.
    /// </summary>
    public static string? NormalizeMain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var label = Regex.Replace(raw.Trim(), @"\s+", " ");
        label = Regex.Replace(label, @"\s+recipes?$", "", RegexOptions.IgnoreCase);

        var canonical = All.FirstOrDefault(c => c.Equals(label, StringComparison.OrdinalIgnoreCase));
        if (canonical != null) return canonical;

        return Synonyms.TryGetValue(label, out var mapped) ? mapped : null;
    }

    /// <summary>
    /// Infers a top-level category from a recipe title, or returns null if no keyword matches.
    /// </summary>
    public static string? InferMainFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;

        foreach (var (category, pattern) in TitleRules)
        {
            if (pattern.IsMatch(title)) return category;
        }

        return null;
    }

    /// <summary>
    /// Cleans a subcategory label: strips punctuation and junk values.
    /// Returns null when nothing usable is left, or when the label is itself a top-level category.
    /// </summary>
    public static string? CleanSub(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var label = Regex.Replace(raw, @"[""'\[\]{},]", " ");
        label = Regex.Replace(label, @"\s+", " ").Trim();

        if (label.Length == 0 || JunkLabels.Contains(label)) return null;
        if (NormalizeMain(label) != null) return null;

        return label.Length > 50 ? label[..50].TrimEnd() : label;
    }
}
