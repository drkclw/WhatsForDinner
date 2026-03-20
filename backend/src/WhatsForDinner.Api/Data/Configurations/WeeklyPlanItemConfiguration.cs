using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsForDinner.Api.Models;

namespace WhatsForDinner.Api.Data.Configurations;

public class WeeklyPlanItemConfiguration : IEntityTypeConfiguration<WeeklyPlanItem>
{
    public void Configure(EntityTypeBuilder<WeeklyPlanItem> builder)
    {
        builder.ToTable("weekly_plan_items");

        builder.HasKey(wpi => wpi.Id);

        builder.Property(wpi => wpi.Id)
            .HasColumnName("id");

        builder.Property(wpi => wpi.WeeklyPlanId)
            .HasColumnName("weekly_plan_id")
            .IsRequired();

        builder.Property(wpi => wpi.RecipeId)
            .HasColumnName("recipe_id")
            .IsRequired();

        builder.Property(wpi => wpi.AddedAt)
            .HasColumnName("added_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Index for fast lookup by weekly plan
        builder.HasIndex(wpi => wpi.WeeklyPlanId)
            .HasDatabaseName("ix_weekly_plan_items_weekly_plan_id");

        // Relationships
        builder.HasOne(wpi => wpi.WeeklyPlan)
            .WithMany(wp => wp.Items)
            .HasForeignKey(wpi => wpi.WeeklyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wpi => wpi.Recipe)
            .WithMany(r => r.WeeklyPlanItems)
            .HasForeignKey(wpi => wpi.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
