using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;

namespace SchoolManagement.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.Id);

            // Basic properties
            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.PasswordHash)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                  .IsRequired();

            entity.Property(e => e.EmailVerified)
                  .IsRequired();

            entity.Property(e => e.PhoneVerified)
                  .IsRequired();

            entity.Property(e => e.LastLoginAt)
                  .IsRequired(false)
                  .HasColumnType("timestamptz");

            entity.Property(e => e.LoginAttempts)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(e => e.LockedUntil)
                  .IsRequired(false)
                  .HasColumnType("timestamptz");

            // UserType enum
            entity.Property(e => e.UserType)
                  .IsRequired()
                  .HasConversion<int>()
                  .HasMaxLength(50);

            // Email Value Object with HasConversion
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(256)
                  .HasColumnName("Email")
                  .HasConversion(
                      email => email.Value,
                      value => new Email(value))
                  .Metadata.SetValueComparer(
                      new ValueComparer<Email>(
                          (left, right) => left != null && right != null && left.Value == right.Value,
                          email => email != null ? email.Value.GetHashCode() : 0,
                          email => email != null ? new Email(email.Value) : null));

            // FullName Value Object as Owned Type
            entity.OwnsOne(e => e.FullName, fullName =>
            {
                fullName.Property(fn => fn.FirstName)
                        .HasColumnName("FirstName")
                        .HasMaxLength(100)
                        .IsRequired();

                fullName.Property(fn => fn.LastName)
                        .HasColumnName("LastName")
                        .HasMaxLength(100)
                        .IsRequired();
            });

            // PhoneNumber Value Object (nullable)
            entity.Property(e => e.PhoneNumber)
                  .HasMaxLength(20)
                  .HasColumnName("PhoneNumber")
                  .HasConversion(
                      phone => phone != null ? phone.Value : null,
                      value => value != null ? new PhoneNumber(value) : null)
                  .Metadata.SetValueComparer(
                      new ValueComparer<PhoneNumber>(
                          (left, right) => (left == null && right == null) ||
                                           (left != null && right != null && left.Value == right.Value),
                          phone => phone != null ? phone.Value.GetHashCode() : 0,
                          phone => phone != null ? new PhoneNumber(phone.Value) : null));

            // Foreign keys
            entity.Property(e => e.StudentId)
                  .IsRequired(false);

            entity.Property(e => e.EmployeeId)
                  .IsRequired(false);

            // Relationships
            entity.HasOne(e => e.Student)
                  .WithOne()
                  .HasForeignKey<User>(e => e.StudentId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);

            entity.HasOne(e => e.Employee)
                  .WithOne()
                  .HasForeignKey<User>(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);

            // RefreshTokens collection with private backing field
            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(u => u.RefreshTokens)
                  .HasField("_refreshTokens")
                  .UsePropertyAccessMode(PropertyAccessMode.Field);

            // UserRoles collection
            entity.HasMany(u => u.UserRoles)
                  .WithOne()
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ✅ Indexes for performance (PostgreSQL syntax)
            entity.HasIndex(e => e.Username)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Username");

            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Users_IsActive");

            entity.HasIndex(e => e.UserType)
                  .HasDatabaseName("IX_Users_UserType");

            // ✅ FIXED: PostgreSQL filtered index syntax
            entity.HasIndex(e => new { e.StudentId, e.IsActive })
                  .HasDatabaseName("IX_Users_StudentId_IsActive")
                  .HasFilter("\"StudentId\" IS NOT NULL"); // Double quotes for PostgreSQL

            entity.HasIndex(e => new { e.EmployeeId, e.IsActive })
                  .HasDatabaseName("IX_Users_EmployeeId_IsActive")
                  .HasFilter("\"EmployeeId\" IS NOT NULL"); // Double quotes for PostgreSQL

            // Concurrency token
            entity.Property(e => e.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();

            // Soft delete query filter
            entity.HasQueryFilter(u => !u.IsDeleted);

            // Audit fields with PostgreSQL timestamptz
            entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);

            entity.Property(e => e.CreatedIP)
                  .HasMaxLength(50);

            entity.Property(e => e.UpdatedAt)
                  .IsRequired(false)
                  .HasColumnType("timestamptz");

            entity.Property(e => e.UpdatedBy)
                  .HasMaxLength(100);

            entity.Property(e => e.CreatedIP)
                  .HasMaxLength(50);

            entity.Property(e => e.IsDeleted)
                  .IsRequired()
                  .HasDefaultValue(false);
        }
    }
}
