namespace Dishwise.Application.Services;

/// <summary>
/// Identifies a recipe page by its address, so the same recipe isn't imported twice
/// whether the link has "www.", a trailing slash, a different scheme or tracking parameters.
/// </summary>
public static class RecipeSourceUrl
{
    public static string Normalize(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            return url.Trim().ToLowerInvariant();

        var host = uri.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal)) host = host[4..];

        // Query strings are dropped: recipe pages are identified by their path
        var path = uri.AbsolutePath.TrimEnd('/').ToLowerInvariant();

        return host + path;
    }
}

/// <summary>
/// Thrown when a URL import would create a recipe that already exists.
/// </summary>
public class DuplicateRecipeException : Exception
{
    public Guid RecipeId { get; }

    public DuplicateRecipeException(Guid recipeId, string title)
        : base($"This recipe has already been imported as \"{title}\".")
    {
        RecipeId = recipeId;
    }
}
