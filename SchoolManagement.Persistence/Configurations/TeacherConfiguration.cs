using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for Teacher entity
    /// </summary>
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            // ===== TABLE & SCHEMA =====
            builder.ToTable("Teachers", "dbo");

            // ===== PRIMARY KEY =====
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedNever(); // Generated in domain

            // ===== VALUE OBJECTS CONFIGURATION =====

            // FullName Value Object
            builder.OwnsOne(t => t.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                name.Ignore(n => n.FullNameString);
            });

            // Email Value Object
            builder.OwnsOne(t => t.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");
            });

            // PhoneNumber Value Object
            builder.OwnsOne(t => t.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");
            });

            // Address Value Object
            builder.OwnsOne(t => t.Address, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                address.Property(a => a.PostalCode)
                    .HasColumnName("PostalCode")
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                address.Property(a => a.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");
            });

            // ===== IDENTITY PROPERTIES =====

            builder.Property(t => t.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(t => t.DateOfJoining)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.DateOfLeaving)
                .HasColumnType("date");

            // ===== PROFESSIONAL PROPERTIES =====

            builder.Property(t => t.Qualification)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(t => t.PriorExperience)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasPrecision(5, 2);

            builder.Property(t => t.Specialization)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(t => t.Salary)
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(t => t.EmploymentType)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(t => t.DepartmentId);

            // ===== ADDITIONAL PROPERTIES =====

            builder.Property(t => t.Gender)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(t => t.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.BloodGroup)
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(t => t.EmergencyContact)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(t => t.EmergencyContactPhone)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(t => t.PhotoUrl)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            // ===== BASE ENTITY PROPERTIES =====

            // ✅ TenantId - REQUIRED for Teacher
            builder.Property(t => t.TenantId)
                .IsRequired();

            // ✅ SchoolId - REQUIRED for Teacher
            builder.Property(t => t.SchoolId)
                .IsRequired();

            // ✅ Audit fields
            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(t => t.CreatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(t => t.CreatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(t => t.UpdatedAt)
                .IsRequired(false)
                .HasColumnType("timestamp with time zone");

            builder.Property(t => t.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(t => t.UpdatedIP)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // ✅ Soft delete
            builder.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.DeletedAt)
                .IsRequired(false)
                .HasColumnType("timestamp with time zone");

            builder.Property(t => t.DeletedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            // ✅ Active status
            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ===== CONCURRENCY TOKEN =====

            // PostgreSQL xmin for row versioning
            builder.Property(t => t.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            // ===== RELATIONSHIPS =====

            // Teacher belongs to School (via BaseEntity.SchoolId)
            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department relationship (many-to-one)
            builder.HasOne(t => t.Department)
                .WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Teachers_Departments");

            // Teaching Assignments relationship (one-to-many with SectionSubject)
            builder.HasMany(t => t.TeachingAssignments)
                .WithOne(ss => ss.Teacher)
                .HasForeignKey(ss => ss.TeacherId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Teachers_SectionSubjects");

            builder.Navigation(t => t.TeachingAssignments)
                .HasField("_teachingAssignments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Class Teacher Sections relationship (one-to-many with Section)
            builder.HasMany(t => t.ClassTeacherSections)
                .WithOne()
                .HasForeignKey(s => s.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Teachers_Sections_ClassTeacher");

            builder.Navigation(t => t.ClassTeacherSections)
                .HasField("_classTeacherSections")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ===== INDEXES =====

            // ✅ Composite unique index on TenantId + SchoolId + EmployeeCode
            builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.EmployeeCode })
                .IsUnique()
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId_EmployeeCode");

            // ✅ Composite unique index on TenantId + SchoolId + Email
            //builder.HasIndex(t => new { t.TenantId, t.SchoolId, EmailValue = EF.Property<string>(t, "Email") })
            //    .IsUnique()
            //    .HasDatabaseName("IX_Teachers_TenantId_SchoolId_Email");

            // ✅ Index on TenantId for tenant isolation
            builder.HasIndex(t => t.TenantId)
                .HasDatabaseName("IX_Teachers_TenantId");

            // ✅ Index on TenantId + SchoolId for school filtering
            builder.HasIndex(t => new { t.TenantId, t.SchoolId })
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId");

            // ✅ Index on IsActive
            builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.IsActive })
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId_IsActive");

            // ✅ Composite index for active, non-deleted teachers
            builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.IsActive, t.IsDeleted })
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId_IsActive_IsDeleted");

            // ✅ Composite index for department filtering
            builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.DepartmentId, t.IsActive })
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId_Department_IsActive");

            // ✅ Index on DateOfJoining for experience calculations
            builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.DateOfJoining })
                .HasDatabaseName("IX_Teachers_TenantId_SchoolId_DateOfJoining");

            // ✅ Index on Name for searching
            //builder.HasIndex(t => new { t.TenantId, t.SchoolId, t.Name.FirstName, t.Name.LastName })
            //    .HasDatabaseName("IX_Teachers_TenantId_SchoolId_Name");

            //builder.HasIndex(t => new
            //{
            //    t.TenantId,
            //    t.SchoolId,
            //    FirstName = EF.Property<string>(t, "Name_FirstName"),
            //    LastName = EF.Property<string>(t, "Name_LastName")
            //})
            //    .HasDatabaseName("IX_Teachers_TenantId_SchoolId_Name");

            // ===== TABLE CHECK CONSTRAINTS =====

            // Ensure PriorExperience is within valid range
            builder.HasCheckConstraint(
                "CK_Teachers_PriorExperience",
                "\"PriorExperience\" >= 0 AND \"PriorExperience\" <= 50"
            );

            // Ensure DateOfLeaving is after DateOfJoining if set
            builder.HasCheckConstraint(
                "CK_Teachers_DateOfLeaving",
                "\"DateOfLeaving\" IS NULL OR \"DateOfLeaving\" >= \"DateOfJoining\""
            );

            // Ensure DateOfBirth is in the past
            builder.HasCheckConstraint(
                "CK_Teachers_DateOfBirth",
                "\"DateOfBirth\" < CURRENT_DATE"
            );

            // Ensure teacher is at least 18 years old
            builder.HasCheckConstraint(
                "CK_Teachers_MinimumAge",
                "EXTRACT(YEAR FROM AGE(CURRENT_DATE, \"DateOfBirth\")) >= 18"
            );

            // Ensure Salary is non-negative
            builder.HasCheckConstraint(
                "CK_Teachers_Salary",
                "\"Salary\" >= 0"
            );

            // ===== QUERY FILTERS =====

            // ✅ Global query filter for soft delete
            builder.HasQueryFilter(t => !t.IsDeleted);

            // ===== IGNORE COMPUTED PROPERTIES =====

            builder.Ignore(t => t.FullName);
            builder.Ignore(t => t.TotalYearsOfExperience);
            builder.Ignore(t => t.IsCurrentlyEmployed);
            builder.Ignore(t => t.IsSeniorTeacher);
            builder.Ignore(t => t.Age);
            builder.Ignore(t => t.DomainEvents);
        }
    }
}