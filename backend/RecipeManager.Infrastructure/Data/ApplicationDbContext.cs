using Microsoft.EntityFrameworkCore;

namespace RecipeManager.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Instruction> Instructions { get; set; }
    public DbSet<RecipeImage> RecipeImages { get; set; }
    public DbSet<Nutrition> Nutritions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Category self-referencing relationship
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Nutrition entity
        modelBuilder.Entity<Nutrition>()
            .HasOne(n => n.Recipe)
            .WithOne(r => r.Nutrition)
            .HasForeignKey<Nutrition>(n => n.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure precision for decimal fields
        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Calories)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Protein)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Carbohydrates)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Fat)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Fiber)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Sugar)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Sodium)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Potassium)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Calcium)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Iron)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Vitamin_A)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Vitamin_C)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Vitamin_D)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Nutrition>()
            .Property(n => n.Cholesterol)
            .HasPrecision(10, 2);
    }
}