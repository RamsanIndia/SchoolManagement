using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;

namespace SchoolManagement.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ===== TABLE & SCHEMA =====
        builder.ToTable("Users", "dbo");

        // ===== PRIMARY KEY =====
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Generated in domain

        // ===== USER IDENTITY PROPERTIES =====

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("varchar(500)");

        builder.Property(e => e.UserType)
            .IsRequired()
            .HasConversion<string>() // ✅ Store as string for readability
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        // ===== SECURITY PROPERTIES =====

        builder.Property(e => e.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.PhoneVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.LastLoginAt)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.LoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.LockedUntil)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        // ===== DOMAIN PROPERTIES =====

        builder.Property(e => e.StudentId)
            .IsRequired(false);

        builder.Property(e => e.EmployeeId)
            .IsRequired(false);

        // ===== VALUE OBJECTS =====

        // Email Value Object
        builder.Property(e => e.Email)
    .IsRequired()
    .HasMaxLength(100)
    .HasColumnName("Email")
    .HasColumnType("varchar(100)")
    .HasConversion(
        email => email.Value,
        value => new Email(value))
    .Metadata.SetValueComparer(
        new ValueComparer<Email>(
            (left, right) => left != null && right != null && left.Value == right.Value,
            email => email != null ? email.Value.GetHashCode() : 0,
            email => email != null ? new Email(email.Value) : null));


        // FullName Value Object as Owned Type
        builder.OwnsOne(e => e.FullName, fullName =>
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
        builder.Property(e => e.PhoneNumber)
    .HasMaxLength(20)
    .HasColumnName("PhoneNumber")
    .HasColumnType("varchar(20)")
    .HasConversion(
        phone => phone != null ? phone.Value : null,
        value => value != null ? new PhoneNumber(value) : null)
    .Metadata.SetValueComparer(
        new ValueComparer<PhoneNumber>(
            (left, right) => (left == null && right == null) ||
                           (left != null && right != null && left.Value == right.Value),
            phone => phone != null ? phone.Value.GetHashCode() : 0,
            phone => phone != null ? new PhoneNumber(phone.Value) : null));

        // ===== BASE ENTITY PROPERTIES =====

        // ✅ TenantId - REQUIRED for User
        builder.Property(e => e.TenantId)
            .IsRequired();

        // ✅ SchoolId - REQUIRED for User
        builder.Property(e => e.SchoolId)
            .IsRequired();

        // ✅ Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(e => e.CreatedIP)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(e => e.UpdatedIP)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        // ✅ Soft delete
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.DeletedBy)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        // ✅ Active status
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ===== CONCURRENCY TOKEN =====

        // PostgreSQL xmin for row versioning
        //builder.Property(e => e.RowVersion)
        //    .HasColumnName("xmin")
        //    .HasColumnType("xid")
        //    .ValueGeneratedOnAddOrUpdate()
        //    .IsConcurrencyToken();

        // ===== RELATIONSHIPS =====

        // ✅ User belongs to School
        builder.HasOne(e => e.School)
            .WithMany(s => s.Users)
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ User optionally linked to Student
        builder.HasOne(e => e.Student)
            .WithOne()
            .HasForeignKey<User>(e => e.StudentId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // ✅ User optionally linked to Employee
        builder.HasOne(e => e.Employee)
            .WithOne()
            .HasForeignKey<User>(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // ✅ User has many RefreshTokens (with private backing field)
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.RefreshTokens)
            .HasField("_refreshTokens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ✅ CRITICAL FIX: User has many UserRoles
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User) // ✅ Specify navigation property!
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== INDEXES =====

        // ✅ Composite unique index on TenantId + Email (email unique per tenant)
        //builder.HasIndex(e => new { e.TenantId, e.Email.Value })
        //    .IsUnique()
        //    .HasDatabaseName("IX_Users_TenantId_Email");

        //builder.HasIndex(e => new { e.TenantId, EmailValue = EF.Property<string>(e, "Email") })
        //        .IsUnique()
        //        .HasDatabaseName("IX_Users_TenantId_Email");

        // ✅ Composite unique index on TenantId + Username
        builder.HasIndex(e => new { e.TenantId, e.Username })
            .IsUnique()
            .HasDatabaseName("IX_Users_TenantId_Username");

        // ✅ Index on TenantId for tenant isolation
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        // ✅ Index on TenantId + SchoolId for filtering users by school
        builder.HasIndex(e => new { e.TenantId, e.SchoolId })
            .HasDatabaseName("IX_Users_TenantId_SchoolId");

        // ✅ Index on UserType within tenant and school context
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.UserType })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_UserType");

        // ✅ Index on IsActive for active users
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.IsActive })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_IsActive");

        // ✅ Composite index for active, non-deleted users
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.IsActive, e.IsDeleted })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_IsActive_IsDeleted");

        // ✅ Filtered index for student users (PostgreSQL syntax)
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.StudentId, e.IsActive })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_StudentId_IsActive")
            .HasFilter("\"StudentId\" IS NOT NULL");

        // ✅ Filtered index for employee users (PostgreSQL syntax)
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.EmployeeId, e.IsActive })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_EmployeeId_IsActive")
            .HasFilter("\"EmployeeId\" IS NOT NULL");

        // ✅ Index on LastLoginAt for recent activity queries
        builder.HasIndex(e => new { e.TenantId, e.SchoolId, e.LastLoginAt })
            .HasDatabaseName("IX_Users_TenantId_SchoolId_LastLoginAt");

        // ===== QUERY FILTERS =====

        // ✅ Global query filter for soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);

        // ===== IGNORE COMPUTED PROPERTIES =====

        builder.Ignore(u => u.IsLockedOut);
        builder.Ignore(u => u.CanLogin);
        builder.Ignore(u => u.HasVerifiedEmail);
        builder.Ignore(u => u.HasVerifiedPhone);
        builder.Ignore(u => u.IsFullyVerified);
        builder.Ignore(u => u.DomainEvents);
    }
}