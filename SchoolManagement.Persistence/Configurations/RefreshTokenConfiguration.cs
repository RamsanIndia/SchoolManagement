using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> entity)
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);

            // Multi-tenant properties
            entity.Property(rt => rt.TenantId)
                  .IsRequired();

            entity.Property(rt => rt.SchoolId)
                  .IsRequired();

            // Token properties
            entity.Property(rt => rt.Token)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(rt => rt.TokenFamily)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(rt => rt.UserId)
                  .IsRequired();

            entity.Property(rt => rt.ExpiryDate)
                  .IsRequired()
                  .HasColumnType("timestamp with time zone");

            // Revocation tracking
            entity.Property(rt => rt.IsRevoked)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(rt => rt.RevokedAt)
                  .HasColumnType("timestamp with time zone");

            entity.Property(rt => rt.RevokedByIp)
                  .HasMaxLength(50);

            entity.Property(rt => rt.RevokedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.ReasonRevoked)
                  .HasMaxLength(500);

            entity.Property(rt => rt.ReplacedByToken)
                  .HasMaxLength(500);

            entity.Property(rt => rt.CreatedIP)
                  .HasMaxLength(50);

            entity.Property(rt => rt.UpdatedIP)
                  .HasMaxLength(50);

            // Multi-tenant indexes (CRITICAL)
            entity.HasIndex(rt => new { rt.TenantId, rt.Token })
                  .IsUnique()
                  .HasDatabaseName("IX_RefreshTokens_TenantId_Token");

            entity.HasIndex(rt => new { rt.TenantId, rt.SchoolId })
                  .HasDatabaseName("IX_RefreshTokens_TenantId_SchoolId");

            entity.HasIndex(rt => rt.TokenFamily)
                  .HasDatabaseName("IX_RefreshTokens_TokenFamily");

            entity.HasIndex(rt => rt.UserId)
                  .HasDatabaseName("IX_RefreshTokens_UserId");

            entity.HasIndex(rt => new { rt.TenantId, rt.SchoolId, rt.UserId, rt.IsRevoked, rt.ExpiryDate })
                  .HasDatabaseName("IX_RefreshTokens_TenantId_SchoolId_UserId_IsRevoked_ExpiryDate");

            entity.HasIndex(rt => new { rt.TenantId, rt.Token, rt.IsRevoked })
                  .HasDatabaseName("IX_RefreshTokens_TenantId_Token_IsRevoked");

            entity.HasIndex(rt => new { rt.TenantId, rt.ExpiryDate, rt.IsRevoked, rt.IsDeleted })
                  .HasDatabaseName("IX_RefreshTokens_TenantId_ExpiryDate_IsRevoked_IsDeleted");

            // Relationships
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            entity.HasOne(rt => rt.Tenant)
                  .WithMany()
                  .HasForeignKey(rt => rt.TenantId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(rt => rt.School)
                  .WithMany()
                  .HasForeignKey(rt => rt.SchoolId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            // Audit fields
            entity.Property(rt => rt.CreatedAt)
                  .IsRequired()
                  .HasColumnType("timestamp with time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(rt => rt.CreatedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.UpdatedAt)
                  .HasColumnType("timestamp with time zone");

            entity.Property(rt => rt.UpdatedBy)
                  .HasMaxLength(100);

            // Soft delete
            entity.Property(rt => rt.IsDeleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            // Concurrency control
            entity.Property(rt => rt.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();

            // Query filter
            entity.HasQueryFilter(rt => !rt.IsDeleted);
        }
    }
}