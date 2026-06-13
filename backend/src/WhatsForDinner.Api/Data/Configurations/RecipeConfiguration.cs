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

        builder.Property(r => r.Preparation)
            .HasColumnName("preparation")
            .HasMaxLength(10000);

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
    }
}
