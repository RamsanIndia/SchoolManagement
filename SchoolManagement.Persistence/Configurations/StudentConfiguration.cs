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
            builder.ToTable("Students");

            // ========== Primary Key ==========
            builder.HasKey(s => s.Id);

            // ========== Properties ==========

            builder.Property(s => s.StudentId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(s => s.StudentId)
                .IsUnique()
                .HasDatabaseName("IX_Students_StudentId");

            builder.Property(s => s.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.MiddleName)
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(s => s.Email)
                .IsUnique()
                .HasDatabaseName("IX_Students_Email");

            builder.Property(s => s.Phone)
                .HasMaxLength(15);

            builder.Property(s => s.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.Gender)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20);

            builder.Property(s => s.ClassId)
                .IsRequired();

            builder.Property(s => s.SectionId)
                .IsRequired();

            builder.Property(s => s.AdmissionNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(s => s.AdmissionNumber)
                .IsUnique()
                .HasDatabaseName("IX_Students_AdmissionNumber");

            builder.Property(s => s.AdmissionDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20)
                .HasDefaultValue(StudentStatus.Active);

            builder.Property(s => s.PhotoUrl)
                .HasMaxLength(500);

            // ========== Value Objects - Owned Types ==========

            // Address Value Object (inline columns - not separate table)
            builder.OwnsOne(s => s.Address, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .IsRequired()
                    .HasMaxLength(200);

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.PostalCode)
                    .HasColumnName("PostalCode")
                    .IsRequired()
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("Country")
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // BiometricInfo Value Object (inline columns)
            builder.OwnsOne(s => s.BiometricInfo, biometric =>
            {
                biometric.Property(b => b.DeviceId)
                    .HasColumnName("BiometricDeviceId")
                    .HasMaxLength(50);

                biometric.Property(b => b.TemplateHash)
                    .HasColumnName("BiometricTemplateHash")
                    .HasMaxLength(500);

                biometric.Property(b => b.EnrollmentDate)
                    .HasColumnName("BiometricEnrollmentDate")
                    .HasColumnType("timestamp");

                biometric.Property(b => b.IsActive)
                    .HasColumnName("BiometricIsActive")
                    .HasDefaultValue(true);
            });

            // ========== Relationships ==========

            // Class relationship (many-to-one)
            builder.HasOne(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Students_Classes");

            // Section relationship (many-to-one)
            builder.HasOne(s => s.Section)
                .WithMany(sec => sec.Students)
                .HasForeignKey(s => s.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Students_Sections");

            // StudentParents relationship (one-to-many)
            builder.HasMany(s => s.StudentParents)
                .WithOne(sp => sp.Student)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_StudentParents");

            // Attendances relationship (one-to-many)
            builder.HasMany(s => s.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_Attendances");

            // FeePayments relationship (one-to-many)
            builder.HasMany(s => s.FeePayments)
                .WithOne(fp => fp.Student)
                .HasForeignKey(fp => fp.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_FeePayments");

            // ExamResults relationship (one-to-many)
            builder.HasMany(s => s.ExamResults)
                .WithOne(er => er.Student)
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_ExamResults");

            // ========== Indexes for Performance ==========

            builder.HasIndex(s => s.Status)
                .HasDatabaseName("IX_Students_Status");

            builder.HasIndex(s => s.ClassId)
                .HasDatabaseName("IX_Students_Class");

            builder.HasIndex(s => s.SectionId)
                .HasDatabaseName("IX_Students_Section");

            builder.HasIndex(s => s.AdmissionDate)
                .HasDatabaseName("IX_Students_AdmissionDate");

            // Composite indexes for common queries
            builder.HasIndex(s => new { s.ClassId, s.SectionId })
                .HasDatabaseName("IX_Students_Class_Section");

            builder.HasIndex(s => new { s.ClassId, s.Status })
                .HasDatabaseName("IX_Students_Class_Status");

            builder.HasIndex(s => new { s.SectionId, s.Status })
                .HasDatabaseName("IX_Students_Section_Status");

            // ========== Audit Fields ==========

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(s => s.UpdatedAt)
                .IsRequired(false);

            builder.Property(s => s.CreatedBy)
                .HasMaxLength(100);

            builder.Property(s => s.UpdatedBy)
                .HasMaxLength(100);

            // Ensure DateOfBirth is in the past
            builder.HasCheckConstraint(
                "CK_Students_DateOfBirth",
                "\"DateOfBirth\" < CURRENT_DATE" // PostgreSQL syntax
            );

            // Ensure AdmissionDate is not in the future
            builder.HasCheckConstraint(
                "CK_Students_AdmissionDate",
                "\"AdmissionDate\" <= CURRENT_DATE" // PostgreSQL syntax
            );

            // Ensure student age is at least 3 years (typical school admission)
            builder.HasCheckConstraint(
                "CK_Students_MinimumAge",
                "EXTRACT(YEAR FROM AGE(CURRENT_DATE, \"DateOfBirth\")) >= 3" // PostgreSQL syntax
            );
        }
    }
}
