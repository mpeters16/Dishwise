namespace Dishwise.Domain.Entities;

public class Nutrition : BaseEntity
{
    public Guid RecipeId { get; set; }
    
    // Macronutrients (per serving)
    public decimal Calories { get; set; }
    public decimal Protein { get; set; } // grams
    public decimal Carbohydrates { get; set; } // grams
    public decimal Fat { get; set; } // grams
    public decimal Fiber { get; set; } // grams
    public decimal Sugar { get; set; } // grams
    
    // Micronutrients
    public decimal? Sodium { get; set; } // milligrams
    public decimal? Potassium { get; set; } // milligrams
    public decimal? Calcium { get; set; } // milligrams
    public decimal? Iron { get; set; } // milligrams
    public decimal? Vitamin_A { get; set; } // IU
    public decimal? Vitamin_C { get; set; } // milligrams
    public decimal? Vitamin_D { get; set; } // micrograms
    
    // Cholesterol
    public decimal? Cholesterol { get; set; } // milligrams
    
    // Navigation
    public Recipe? Recipe { get; set; }
}
