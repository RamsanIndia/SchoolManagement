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
            builder.ToTable("Teachers");

            // Primary Key
            builder.HasKey(t => t.Id);

            // ========== Value Objects Configuration ==========

            // FullName Value Object
            builder.OwnsOne(t => t.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .IsRequired()
                    .HasMaxLength(100);

                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .IsRequired()
                    .HasMaxLength(100);

                // Computed properties don't need explicit Ignore in owned types
                name.Ignore(n => n.FullNameString);
            });

            // Email Value Object
            builder.OwnsOne(t => t.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Teachers_Email");
            });

            // PhoneNumber Value Object
            builder.OwnsOne(t => t.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .IsRequired()
                    .HasMaxLength(15);
            });

            // Address Value Object
            builder.OwnsOne(t => t.Address, address =>
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

            // ========== Primitive Properties ==========

            builder.Property(t => t.EmployeeId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(t => t.EmployeeId)
                .IsUnique()
                .HasDatabaseName("IX_Teachers_EmployeeId");

            builder.Property(t => t.DateOfJoining)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.DateOfLeaving)
                .HasColumnType("date");

            builder.Property(t => t.Qualification)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Experience)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasPrecision(5, 2);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ========== Relationships ==========

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

            // Class Teacher Sections relationship (one-to-many with Section)
            builder.HasMany(t => t.ClassTeacherSections)
                .WithOne()
                .HasForeignKey(s => s.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Teachers_Sections_ClassTeacher");

            // ========== Audit Fields ==========

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(t => t.UpdatedAt)
                .IsRequired(false);

            builder.Property(t => t.CreatedBy)
                .HasMaxLength(100);

            builder.Property(t => t.UpdatedBy)
                .HasMaxLength(100);

            // ========== Indexes for Performance ==========

            // Index on IsActive for filtering active teachers
            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_Teachers_IsActive");

            // Composite index for department filtering
            builder.HasIndex(t => new { t.DepartmentId, t.IsActive })
                .HasDatabaseName("IX_Teachers_Department_IsActive");

            // Index on DateOfJoining for experience calculations
            builder.HasIndex(t => t.DateOfJoining)
                .HasDatabaseName("IX_Teachers_DateOfJoining");

            // ========== Ignore Computed Properties ==========

            // These are computed properties/methods that shouldn't be mapped to database
            builder.Ignore(t => t.FullName);

            // ========== Table Check Constraints ==========

            // Ensure Experience is within valid range
            builder.HasCheckConstraint(
                "CK_Teachers_Experience",
                "\"Experience\" >= 0 AND \"Experience\" <= 50" // PostgreSQL syntax
            );

            // Ensure DateOfLeaving is after DateOfJoining if set
            builder.HasCheckConstraint(
                "CK_Teachers_DateOfLeaving",
                "\"DateOfLeaving\" IS NULL OR \"DateOfLeaving\" >= \"DateOfJoining\"" // PostgreSQL syntax
            );
        }
    }
}
