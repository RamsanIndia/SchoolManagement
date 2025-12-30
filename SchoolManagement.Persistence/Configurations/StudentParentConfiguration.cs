using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
    {
        public void Configure(EntityTypeBuilder<StudentParent> builder)
        {
            builder.ToTable("StudentParents");

            // ========== Primary Key ==========
            builder.HasKey(sp => sp.Id);

            // ========== Properties ==========

            builder.Property(sp => sp.StudentId)
                .IsRequired();

            builder.Property(sp => sp.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sp => sp.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sp => sp.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(sp => sp.Email)
                .IsUnique()
                .HasDatabaseName("IX_StudentParents_Email");

            builder.Property(sp => sp.Phone)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(sp => sp.Relationship)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(50);

            builder.Property(sp => sp.IsPrimaryContact)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sp => sp.IsEmergencyContact)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sp => sp.Occupation)
                .HasMaxLength(100);

            builder.Property(sp => sp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ========== Value Objects - Owned Types ==========

            // Address Value Object
            builder.OwnsOne(sp => sp.Address, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .HasMaxLength(200);

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .HasMaxLength(100);

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .HasMaxLength(100);

                address.Property(a => a.PostalCode)
                    .HasColumnName("PostalCode")
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100);
            });

            // ========== Relationships ==========

            // Student relationship (many-to-one)
            builder.HasOne(sp => sp.Student)
                .WithMany(s => s.StudentParents)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_StudentParents_Students");

            // ========== Indexes for Performance ==========

            builder.HasIndex(sp => sp.StudentId)
                .HasDatabaseName("IX_StudentParents_Student");

            builder.HasIndex(sp => sp.IsPrimaryContact)
                .HasDatabaseName("IX_StudentParents_PrimaryContact");

            builder.HasIndex(sp => sp.IsEmergencyContact)
                .HasDatabaseName("IX_StudentParents_EmergencyContact");

            builder.HasIndex(sp => sp.IsActive)
                .HasDatabaseName("IX_StudentParents_IsActive");

            builder.HasIndex(sp => sp.Relationship)
                .HasDatabaseName("IX_StudentParents_Relationship");

            // Composite indexes for common queries
            builder.HasIndex(sp => new { sp.StudentId, sp.IsActive })
                .HasDatabaseName("IX_StudentParents_Student_IsActive");

            builder.HasIndex(sp => new { sp.StudentId, sp.IsPrimaryContact })
                .HasDatabaseName("IX_StudentParents_Student_PrimaryContact");

            builder.HasIndex(sp => new { sp.StudentId, sp.IsEmergencyContact })
                .HasDatabaseName("IX_StudentParents_Student_EmergencyContact");

            // ========== Audit Fields ==========

            builder.Property(sp => sp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(sp => sp.UpdatedAt)
                .IsRequired(false);

            builder.Property(sp => sp.CreatedBy)
                .HasMaxLength(100);

            builder.Property(sp => sp.UpdatedBy)
                .HasMaxLength(100);

            // ========== Concurrency Token ==========
            // ✅ REMOVED - Configured globally in DbContext using PostgreSQL xmin

            // ========== Check Constraints ==========

            // Ensure one primary contact per student
            // This would be enforced at the application level via a query check
            // or a database trigger for strict enforcement
        }
    }
}
