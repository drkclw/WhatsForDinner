using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsForDinner.Api.Models;

namespace WhatsForDinner.Api.Data.Configurations;

public class WeeklyPlanConfiguration : IEntityTypeConfiguration<WeeklyPlan>
{
    public void Configure(EntityTypeBuilder<WeeklyPlan> builder)
    {
        builder.ToTable("weekly_plans");

        builder.HasKey(wp => wp.Id);

        builder.Property(wp => wp.Id)
            .HasColumnName("id");

        builder.Property(wp => wp.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(wp => wp.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(wp => wp.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Unique constraint - one plan per user
        builder.HasIndex(wp => wp.UserId)
            .IsUnique()
            .HasDatabaseName("uq_weekly_plan_user_id");

        // Seed data - weekly plan for default user
        builder.HasData(new WeeklyPlan
        {
            Id = 1,
            UserId = 1,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
