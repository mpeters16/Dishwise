namespace Dishwise.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public string Url { get; set; } = "";
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? Size { get; set; }
    public bool IsPrimary { get; set; }
    /// <summary>When set, this image belongs to the instruction step with this 1-based order number.</summary>
    public int? StepOrder { get; set; }
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;
}