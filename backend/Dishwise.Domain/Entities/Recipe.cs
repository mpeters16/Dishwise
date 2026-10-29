using System.ComponentModel.DataAnnotations.Schema;

namespace Dishwise.Domain.Entities;

public class Recipe : BaseEntity
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int CookTimeMinutes { get; set; } // in minutes
    public int PrepTimeMinutes { get; set; } // in minutes
    public int? Servings { get; set; } // null when the source doesn't say
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public ICollection<Instruction> Instructions { get; set; } = new List<Instruction>();
    public ICollection<RecipeImage> Images { get; set; } = new List<RecipeImage>();
    public Nutrition? Nutrition { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    [NotMapped]
    public Guid ImageId { get; set; }

    public string? SourceUrl { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}