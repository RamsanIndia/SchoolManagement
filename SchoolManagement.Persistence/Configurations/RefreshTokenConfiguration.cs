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

            // ✅ Token value (hashed or plain depending on your implementation)
            entity.Property(rt => rt.Token)
                  .IsRequired()
                  .HasMaxLength(500);

            // ✅ Token Family for rotation tracking (SECURITY FEATURE)
            entity.Property(rt => rt.TokenFamily)
                  .IsRequired()
                  .HasMaxLength(500);

            // ✅ User relationship
            entity.Property(rt => rt.UserId)
                  .IsRequired();

            // ✅ Expiry date
            entity.Property(rt => rt.ExpiryDate)
                  .IsRequired()
                  .HasColumnType("timestamptz");

            // ✅ Revocation tracking
            entity.Property(rt => rt.IsRevoked)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(rt => rt.RevokedAt)
                  .HasColumnType("timestamptz");

            entity.Property(rt => rt.RevokedByIp)
                  .HasMaxLength(50);

            entity.Property(rt => rt.ReasonRevoked)
                  .HasMaxLength(500);

            // ✅ Token replacement tracking
            entity.Property(rt => rt.ReplacedByToken)
                  .HasMaxLength(500);

            // ✅ IP address tracking for security
            entity.Property(rt => rt.CreatedIP)
                  .HasMaxLength(50);

            // ✅ Indexes for performance and security queries
            entity.HasIndex(rt => rt.Token)
                  .IsUnique()
                  .HasDatabaseName("IX_RefreshTokens_Token");

            entity.HasIndex(rt => rt.TokenFamily)
                  .HasDatabaseName("IX_RefreshTokens_TokenFamily");

            entity.HasIndex(rt => rt.UserId)
                  .HasDatabaseName("IX_RefreshTokens_UserId");

            // ✅ Composite index for common security queries
            entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiryDate })
                  .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiryDate");

            // ✅ Index for token reuse detection (critical for security)
            entity.HasIndex(rt => new { rt.Token, rt.IsRevoked })
                  .HasDatabaseName("IX_RefreshTokens_Token_IsRevoked");

            // ✅ Index for cleanup operations
            entity.HasIndex(rt => new { rt.ExpiryDate, rt.IsRevoked, rt.IsDeleted })
                  .HasDatabaseName("IX_RefreshTokens_ExpiryDate_IsRevoked_IsDeleted");

            // ✅ Relationship with User
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            // ✅ Audit fields from BaseEntity (using CreatedDate not CreatedAt)
            entity.Property(rt => rt.CreatedAt)
                  .IsRequired()
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(rt => rt.CreatedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.CreatedIP)
                  .HasMaxLength(50);

            entity.Property(rt => rt.UpdatedAt)
                  .HasColumnType("timestamptz");

            entity.Property(rt => rt.UpdatedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.CreatedIP)
                  .HasMaxLength(50);

            // ✅ Soft delete
            entity.Property(rt => rt.IsDeleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            // ✅ Concurrency control
            entity.Property(rt => rt.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();

            // ✅ Soft delete query filter
            entity.HasQueryFilter(rt => !rt.IsDeleted);
        }
    }
}
