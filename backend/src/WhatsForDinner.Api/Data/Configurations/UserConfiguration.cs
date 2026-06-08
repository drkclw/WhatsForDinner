using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsForDinner.Api.Models;

namespace WhatsForDinner.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.GoogleId)
            .HasColumnName("google_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PictureUrl)
            .HasColumnName("picture_url")
            .HasMaxLength(2048);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(u => u.GoogleId)
            .IsUnique()
            .HasDatabaseName("ux_users_google_id");

        // Relationships
        builder.HasMany(u => u.Recipes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.WeeklyPlan)
            .WithOne(wp => wp.User)
            .HasForeignKey<WeeklyPlan>(wp => wp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
