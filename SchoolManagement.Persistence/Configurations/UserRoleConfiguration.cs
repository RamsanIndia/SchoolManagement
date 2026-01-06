using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for UserRole join table
/// Clean configuration with proper relationships
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        // ========================================================================
        // PRIMARY KEY
        // ========================================================================
        // Using Id from BaseEntity as primary key
        builder.HasKey(ur => ur.Id);

        // Add unique constraint on UserId + RoleId to prevent duplicate assignments
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_UserId_RoleId_Unique");

        // ========================================================================
        // PROPERTIES
        // ========================================================================

        builder.Property(ur => ur.UserId)
            .IsRequired()
            .HasColumnName("UserId");

        builder.Property(ur => ur.RoleId)
            .IsRequired()
            .HasColumnName("RoleId");

        builder.Property(ur => ur.AssignedAt)
            .IsRequired();

        builder.Property(ur => ur.AssignedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ur => ur.ExpiresAt)
            .IsRequired(false);

        builder.Property(ur => ur.RevokedAt)
            .IsRequired(false);

        builder.Property(ur => ur.RevokedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(ur => ur.RevokeReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(ur => ur.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ========================================================================
        // RELATIONSHIPS
        // ========================================================================

        // User relationship
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Role relationship
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================================================
        // INDEXES FOR PERFORMANCE
        // ========================================================================

        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRoles_UserId");

        builder.HasIndex(ur => ur.RoleId)
            .HasDatabaseName("IX_UserRoles_RoleId");

        builder.HasIndex(ur => ur.IsActive)
            .HasDatabaseName("IX_UserRoles_IsActive");

        builder.HasIndex(ur => new { ur.UserId, ur.IsActive })
            .HasDatabaseName("IX_UserRoles_UserId_IsActive");

        builder.HasIndex(ur => ur.ExpiresAt)
            .HasDatabaseName("IX_UserRoles_ExpiresAt")
            .HasFilter("\"ExpiresAt\" IS NOT NULL");

        // ========================================================================
        // CONCURRENCY TOKEN
        // ========================================================================

        builder.Property(ur => ur.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}