using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;

namespace SchoolManagement.Persistence.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // ===== TABLE & SCHEMA =====
            builder.ToTable("Students", "dbo");

            // ===== PRIMARY KEY =====
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .IsRequired()
                .ValueGeneratedNever(); // Generated in domain

            // ===== IDENTITY PROPERTIES =====

            builder.Property(s => s.StudentCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(s => s.AdmissionNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(s => s.AdmissionDate)
                .IsRequired()
                .HasColumnType("date");

            // ===== PERSONAL INFORMATION =====

            builder.Property(s => s.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.MiddleName)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.Gender)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            // ===== CONTACT INFORMATION =====

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(s => s.Phone)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            // ===== ACADEMIC INFORMATION =====

            builder.Property(s => s.ClassId)
                .IsRequired();

            builder.Property(s => s.SectionId)
                .IsRequired();

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)")
                .HasDefaultValue(StudentStatus.Active);

            builder.Property(s => s.RollNumber)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            // ===== ADDITIONAL INFORMATION =====

            builder.Property(s => s.PhotoUrl)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            builder.Property(s => s.BloodGroup)
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(s => s.Nationality)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(s => s.Religion)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // ===== VALUE OBJECTS - OWNED TYPES =====

            // Address Value Object (inline columns)
            builder.OwnsOne(s => s.Address, address =>
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

            // BiometricInfo Value Object (inline columns)
            builder.OwnsOne(s => s.BiometricInfo, biometric =>
            {
                biometric.Property(b => b.DeviceId)
                    .HasColumnName("BiometricDeviceId")
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                biometric.Property(b => b.TemplateHash)
                    .HasColumnName("BiometricTemplateHash")
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                biometric.Property(b => b.EnrollmentDate)
                    .HasColumnName("BiometricEnrollmentDate")
                    .HasColumnType("timestamp with time zone");

                biometric.Property(b => b.IsActive)
                    .HasColumnName("BiometricIsActive")
                    .HasDefaultValue(true);
            });

            // ===== BASE ENTITY PROPERTIES =====

            // ✅ TenantId - REQUIRED for Student
            builder.Property(s => s.TenantId)
                .IsRequired();

            // ✅ SchoolId - REQUIRED for Student
            builder.Property(s => s.SchoolId)
                .IsRequired();

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
                .IsRequired(false)
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
                .IsRequired(false)
                .HasColumnType("timestamp with time zone");

            builder.Property(s => s.DeletedBy)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            // ✅ Active status
            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ===== CONCURRENCY TOKEN =====

            // PostgreSQL xmin for row versioning
            builder.Property(s => s.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            // ===== RELATIONSHIPS =====

            // Student belongs to School (via BaseEntity.SchoolId)
            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student belongs to Class
            builder.HasOne(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Students_Classes");

            // Student belongs to Section
            builder.HasOne(s => s.Section)
                .WithMany(sec => sec.Students)
                .HasForeignKey(s => s.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Students_Sections");

            // Student has many StudentParents
            builder.HasMany(s => s.StudentParents)
                .WithOne(sp => sp.Student)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_StudentParents");

            builder.Navigation(s => s.StudentParents)
                .HasField("_studentParents")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Student has many Attendances
            builder.HasMany(s => s.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_Attendances");

            builder.Navigation(s => s.Attendances)
                .HasField("_attendances")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Student has many FeePayments
            builder.HasMany(s => s.FeePayments)
                .WithOne(fp => fp.Student)
                .HasForeignKey(fp => fp.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_FeePayments");

            builder.Navigation(s => s.FeePayments)
                .HasField("_feePayments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Student has many ExamResults
            builder.HasMany(s => s.ExamResults)
                .WithOne(er => er.Student)
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_ExamResults");

            builder.Navigation(s => s.ExamResults)
                .HasField("_examResults")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ===== INDEXES =====

            // ✅ Composite unique index on TenantId + SchoolId + StudentCode
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.StudentCode })
                .IsUnique()
                .HasDatabaseName("IX_Students_TenantId_SchoolId_StudentCode");

            // ✅ Composite unique index on TenantId + SchoolId + AdmissionNumber
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.AdmissionNumber })
                .IsUnique()
                .HasDatabaseName("IX_Students_TenantId_SchoolId_AdmissionNumber");

            // ✅ Composite unique index on TenantId + SchoolId + Email
            //builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.Email })
            //    .IsUnique()
            //    .HasDatabaseName("IX_Students_TenantId_SchoolId_Email");

            //builder.HasIndex(s => new { s.TenantId, EmailValue = EF.Property<string>(s, "Email") })
            //    .IsUnique()
            //    .HasDatabaseName("IX_Students_TenantId_SchoolId_Email");

            // ✅ Index on TenantId for tenant isolation
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("IX_Students_TenantId");

            // ✅ Index on TenantId + SchoolId for school filtering
            builder.HasIndex(s => new { s.TenantId, s.SchoolId })
                .HasDatabaseName("IX_Students_TenantId_SchoolId");

            // ✅ Index on Status
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.Status })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_Status");

            // ✅ Index on ClassId
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.ClassId })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_ClassId");

            // ✅ Index on SectionId
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.SectionId })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_SectionId");

            // ✅ Composite index for common queries
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.ClassId, s.SectionId })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_Class_Section");

            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.ClassId, s.Status })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_Class_Status");

            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.IsActive, s.IsDeleted })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_IsActive_IsDeleted");

            // ✅ Index on AdmissionDate
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.AdmissionDate })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_AdmissionDate");

            // ✅ Index on Name for searching
            builder.HasIndex(s => new { s.TenantId, s.SchoolId, s.FirstName, s.LastName })
                .HasDatabaseName("IX_Students_TenantId_SchoolId_Name");

            // ===== CHECK CONSTRAINTS =====

            // Ensure DateOfBirth is in the past
            builder.HasCheckConstraint(
                "CK_Students_DateOfBirth",
                "\"DateOfBirth\" < CURRENT_DATE"
            );

            // Ensure AdmissionDate is not in the future
            builder.HasCheckConstraint(
                "CK_Students_AdmissionDate",
                "\"AdmissionDate\" <= CURRENT_DATE"
            );

            // Ensure student age is at least 3 years
            builder.HasCheckConstraint(
                "CK_Students_MinimumAge",
                "EXTRACT(YEAR FROM AGE(CURRENT_DATE, \"DateOfBirth\")) >= 3"
            );

            // ===== QUERY FILTERS =====

            // ✅ Global query filter for soft delete
            builder.HasQueryFilter(s => !s.IsDeleted);

            // ===== IGNORE COMPUTED PROPERTIES =====

            builder.Ignore(s => s.FullName);
            builder.Ignore(s => s.Age);
            builder.Ignore(s => s.IsEnrolled);
            builder.Ignore(s => s.DomainEvents);
        }
    }
}