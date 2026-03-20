using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsForDinner.Api.Models;

namespace WhatsForDinner.Api.Data.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(r => r.Ingredients)
            .HasColumnName("ingredients")
            .HasMaxLength(2000);

        builder.Property(r => r.CookTimeMinutes)
            .HasColumnName("cook_time_minutes");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Index for fast lookup by user
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_recipes_user_id");

        // Seed data - 5 predefined recipes
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        builder.HasData(
            new Recipe
            {
                Id = 1,
                UserId = 1,
                Name = "Spaghetti Bolognese",
                Description = "Classic Italian pasta with meat sauce",
                Ingredients = "Spaghetti\nGround beef\nTomato sauce\nOnion\nGarlic\nOlive oil\nSalt\nPepper\nParmesan",
                CookTimeMinutes = 45,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Recipe
            {
                Id = 2,
                UserId = 1,
                Name = "Grilled Chicken Salad",
                Description = "Fresh salad with grilled chicken breast",
                Ingredients = "Chicken breast\nMixed greens\nCherry tomatoes\nCucumber\nRed onion\nOlive oil\nLemon juice",
                CookTimeMinutes = 25,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Recipe
            {
                Id = 3,
                UserId = 1,
                Name = "Vegetable Stir Fry",
                Description = "Quick and healthy Asian-inspired dish",
                Ingredients = "Broccoli\nBell peppers\nCarrots\nSnap peas\nSoy sauce\nGarlic\nGinger\nSesame oil\nRice",
                CookTimeMinutes = 20,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Recipe
            {
                Id = 4,
                UserId = 1,
                Name = "Beef Tacos",
                Description = "Mexican-style tacos with seasoned beef",
                Ingredients = "Ground beef\nTaco shells\nLettuce\nTomatoes\nCheese\nSour cream\nTaco seasoning",
                CookTimeMinutes = 30,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Recipe
            {
                Id = 5,
                UserId = 1,
                Name = "Margherita Pizza",
                Description = "Simple Italian pizza with fresh ingredients",
                Ingredients = "Pizza dough\nTomato sauce\nFresh mozzarella\nBasil\nOlive oil",
                CookTimeMinutes = 25,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}
