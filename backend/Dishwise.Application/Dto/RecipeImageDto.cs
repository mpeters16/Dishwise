namespace Dishwise.Application.Dto;

public class RecipeImageDto
{
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? Size { get; set; }
    public bool IsPrimary { get; set; }
    /// <summary>When set, this image belongs to the instruction step with this 1-based order number.</summary>
    public int? StepOrder { get; set; }
}