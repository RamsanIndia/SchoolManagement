using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;

namespace SchoolManagement.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            // ========== Primary Key ==========
            builder.HasKey(e => e.Id);

            // ========== Properties ==========

            builder.Property(e => e.EmployeeId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(e => e.EmployeeId)
                .IsUnique()
                .HasDatabaseName("IX_Employees_EmployeeId");

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MiddleName)
                .HasMaxLength(100);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Employees_Email");

            builder.Property(e => e.Phone)
                .HasMaxLength(15);

            builder.Property(e => e.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(e => e.Gender)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20);

            builder.Property(e => e.JoiningDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(e => e.DepartmentId)
                .IsRequired();

            builder.Property(e => e.DesignationId)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20)
                .HasDefaultValue(EmployeeStatus.Active);

            builder.Property(e => e.EmploymentType)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20);

            builder.Property(e => e.PhotoUrl)
                .HasMaxLength(500);

            // ========== Value Objects - Owned Types ==========

            // Address Value Object
            builder.OwnsOne(e => e.Address, address =>
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

            // Salary Value Object
            builder.OwnsOne(e => e.SalaryInfo, salary =>
            {
                salary.Property(s => s.BasicSalary)
                    .HasColumnName("BasicSalary")
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2)
                    .IsRequired();

                salary.Property(s => s.HRA)
                    .HasColumnName("HRA")
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2)
                    .IsRequired();

                salary.Property(s => s.Allowances)
                    .HasColumnName("Allowances")
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2)
                    .IsRequired();

                salary.Property(s => s.Deductions)
                    .HasColumnName("Deductions")
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2)
                    .IsRequired();

                // Computed properties don't need explicit Ignore - they're not mapped by default
                // But keeping for clarity
                salary.Ignore(s => s.GrossSalary);
                salary.Ignore(s => s.NetSalary);
            });

            // BiometricInfo Value Object
            builder.OwnsOne(e => e.BiometricInfo, biometric =>
            {
                biometric.Property(b => b.DeviceId)
                    .HasColumnName("BiometricDeviceId")
                    .HasMaxLength(50);

                biometric.Property(b => b.TemplateHash)
                    .HasColumnName("BiometricTemplateHash")
                    .HasMaxLength(500);

                biometric.Property(b => b.EnrollmentDate)
                    .HasColumnName("BiometricEnrollmentDate")
                    .HasColumnType("timestamp"); // PostgreSQL syntax

                biometric.Property(b => b.IsActive)
                    .HasColumnName("BiometricIsActive")
                    .HasDefaultValue(true);
            });

            // ========== Relationships ==========

            // Department relationship (many-to-one)
            builder.HasOne(e => e.Department)
                .WithMany() // Department doesn't have Employees collection in your code
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employees_Departments");

            // Designation relationship (many-to-one)
            builder.HasOne(e => e.Designation)
                .WithMany() // Designation doesn't have Employees collection
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employees_Designations");

            // Attendances relationship (one-to-many)
            builder.HasMany(e => e.Attendances)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Employees_Attendances");

            // LeaveApplications relationship (one-to-many)
            builder.HasMany(e => e.LeaveApplications)
                .WithOne(la => la.Employee)
                .HasForeignKey(la => la.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Employees_LeaveApplications");

            // PerformanceReviews relationship (one-to-many)
            builder.HasMany(e => e.PerformanceReviews)
                .WithOne(pr => pr.Employee)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Employees_PerformanceReviews");

            // PayrollRecords relationship (one-to-many)
            builder.HasMany(e => e.PayrollRecords)
                .WithOne(pr => pr.Employee)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Employees_PayrollRecords");

            // ========== Indexes for Performance ==========

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Employees_Status");

            builder.HasIndex(e => e.DepartmentId)
                .HasDatabaseName("IX_Employees_Department");

            builder.HasIndex(e => e.DesignationId)
                .HasDatabaseName("IX_Employees_Designation");

            builder.HasIndex(e => e.JoiningDate)
                .HasDatabaseName("IX_Employees_JoiningDate");

            builder.HasIndex(e => new { e.DepartmentId, e.Status })
                .HasDatabaseName("IX_Employees_Department_Status");

            // ========== Audit Fields ==========

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(e => e.UpdatedAt)
                .IsRequired(false);

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // ========== Concurrency Token ==========
            // ✅ REMOVED - Configured globally in DbContext using PostgreSQL xmin

            // ========== Check Constraints ==========

            // Ensure DateOfBirth is in the past
            builder.HasCheckConstraint(
                "CK_Employees_DateOfBirth",
                "\"DateOfBirth\" < CURRENT_DATE" // PostgreSQL syntax
            );

            // Ensure JoiningDate is not in the future
            builder.HasCheckConstraint(
                "CK_Employees_JoiningDate",
                "\"JoiningDate\" <= CURRENT_DATE" // PostgreSQL syntax
            );
        }
    }
}
