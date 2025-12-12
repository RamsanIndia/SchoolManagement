// Infrastructure/Persistence/Configurations/UserRoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => ur.Id);

            builder.Property(ur => ur.UserId)
                .IsRequired();

            builder.Property(ur => ur.RoleId)
                .IsRequired();

            builder.Property(ur => ur.AssignedAt)
                .IsRequired();

            builder.Property(ur => ur.AssignedBy)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(ur => ur.ExpiresAt)
                .IsRequired(false);  // Make nullable

            builder.Property(ur => ur.RevokedAt)
                .IsRequired(false);  // Make nullable

            builder.Property(ur => ur.RevokedBy)
                .HasMaxLength(100)
                .IsRequired(false);  // Make nullable - ADDED

            builder.Property(ur => ur.RevokeReason)
                .HasMaxLength(500)
                .IsRequired(false);  // Make nullable - ADDED

            builder.Property(ur => ur.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.IsActive });
            builder.HasIndex(ur => ur.ExpiresAt);
            builder.HasIndex(ur => ur.IsActive);

            // Row Version for concurrency
            builder.Property(ur => ur.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
