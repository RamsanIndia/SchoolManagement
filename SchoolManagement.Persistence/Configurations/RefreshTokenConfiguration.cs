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

            entity.Property(rt => rt.Token)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.HasIndex(rt => rt.Token)
                  .IsUnique()
                  .HasDatabaseName("IX_RefreshTokens_Token");

            entity.Property(rt => rt.UserId)
                  .IsRequired();

            entity.HasIndex(rt => rt.UserId)
                  .HasDatabaseName("IX_RefreshTokens_UserId");

            // ✅ PostgreSQL date/time
            entity.Property(rt => rt.ExpiryDate)
                  .IsRequired()
                  .HasColumnType("timestamptz");

            entity.Property(rt => rt.IsRevoked)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(rt => rt.RevokedAt)
                  .HasColumnType("timestamptz");

            entity.Property(rt => rt.RevokedByIp)
                  .HasMaxLength(50);

            entity.Property(rt => rt.ReasonRevoked)
                  .HasMaxLength(500);

            entity.Property(rt => rt.ReplacedByToken)
                  .HasMaxLength(500);

            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiryDate })
                  .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiryDate");

            // ✅ Audit fields (PostgreSQL)
            entity.Property(rt => rt.CreatedAt)
                  .IsRequired()
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(rt => rt.CreatedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.UpdatedAt)
                  .HasColumnType("timestamptz");

            entity.Property(rt => rt.UpdatedBy)
                  .HasMaxLength(100);

            entity.Property(rt => rt.IsDeleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            // ✅ PostgreSQL concurrency control
            entity.Property(rt => rt.RowVersion)
                  .IsConcurrencyToken()
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasQueryFilter(rt => !rt.IsDeleted);
        }
    }
}
