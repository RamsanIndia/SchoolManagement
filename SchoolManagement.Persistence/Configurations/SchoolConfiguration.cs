using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;

namespace SchoolManagement.Persistence.Configurations
{
    public class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            // ===== TABLE & SCHEMA =====
            builder.ToTable("Schools", "school");

            // ===== PRIMARY KEY =====
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .IsRequired()
                .ValueGeneratedNever(); // Generated in domain

            // ===== SCHOOL PROPERTIES =====

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(s => s.Type)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .HasDefaultValue(SchoolType.Primary);

            builder.Property(s => s.Address)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            builder.Property(s => s.ContactPhone)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(s => s.ContactEmail)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.MaxStudentCapacity)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .HasDefaultValue(SchoolStatus.Active);

            builder.Property(s => s.FoundedDate)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // ===== BASE ENTITY PROPERTIES =====

            // ✅ TenantId - REQUIRED for School
            builder.Property(s => s.TenantId)
                .IsRequired();

            // ✅ SchoolId - NULL for School entity (it IS the school)
            builder.Property(s => s.SchoolId)
                .IsRequired(false); // Nullable

            // ✅ Audit fields
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(s => s.CreatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.CreatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(s => s.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(s => s.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.UpdatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // ✅ Soft delete
            builder.Property(s => s.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.DeletedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(s => s.DeletedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            // ✅ Active status
            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ===== INDEXES =====

            // ✅ Composite unique index on TenantId + Code (school code unique per tenant)
            builder.HasIndex(s => new { s.TenantId, s.Code })
                .IsUnique()
                .HasDatabaseName("IX_Schools_TenantId_Code");

            // ✅ Index on TenantId for tenant isolation
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("IX_Schools_TenantId");

            // ✅ Index on Name for searching within tenant
            builder.HasIndex(s => new { s.TenantId, s.Name })
                .HasDatabaseName("IX_Schools_TenantId_Name");

            // ✅ Index on Status for filtering
            builder.HasIndex(s => new { s.TenantId, s.Status })
                .HasDatabaseName("IX_Schools_TenantId_Status");

            // ✅ Index on IsActive for active schools
            builder.HasIndex(s => new { s.TenantId, s.IsActive })
                .HasDatabaseName("IX_Schools_TenantId_IsActive");

            // ✅ Composite index for active, non-deleted schools
            builder.HasIndex(s => new { s.TenantId, s.IsActive, s.IsDeleted })
                .HasDatabaseName("IX_Schools_TenantId_IsActive_IsDeleted");

            // ===== RELATIONSHIPS =====

            // ✅ School belongs to Tenant
            builder.HasOne(s => s.Tenant)  // Reference the navigation property
                .WithMany(t => t.Schools)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // ✅ School has many Users
            builder.HasMany(s => s.Users)
                .WithOne(u => u.School)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Users can exist without school temporarily

            // ===== QUERY FILTERS =====

            // ✅ CORRECTED: Global query filter for soft delete only
            // Remove the SchoolId check - School entity should have SchoolId = null
            builder.HasQueryFilter(s => !s.IsDeleted);

            // ===== CONCURRENCY TOKEN =====

            // ✅ PostgreSQL xmin for row versioning
            builder.Property(s => s.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            // ===== IGNORE COMPUTED PROPERTIES =====

            builder.Ignore(s => s.CurrentStudentCount);
            builder.Ignore(s => s.CurrentEmployeeCount);
            builder.Ignore(s => s.CurrentNonTeachingStaffCount);
            builder.Ignore(s => s.TotalStaffCount);
            builder.Ignore(s => s.CapacityUtilization);
            builder.Ignore(s => s.IsAtCapacity);
            builder.Ignore(s => s.IsOperational);
            builder.Ignore(s => s.StudentIds);
            builder.Ignore(s => s.EmployeeIds);
            builder.Ignore(s => s.ClassroomIds);
            builder.Ignore(s => s.NonTeachingStaffIds);
            builder.Ignore(s => s.DomainEvents);
        }
    }
}