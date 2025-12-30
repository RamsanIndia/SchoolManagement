using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable("Classes");

            // ========== Primary Key ==========
            builder.HasKey(c => c.Id);

            // ========== Properties ==========

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("IX_Classes_Code");

            builder.Property(c => c.Grade)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Capacity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.AcademicYearId)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ========== Relationships ==========

            // AcademicYear relationship (many-to-one)
            builder.HasOne(c => c.AcademicYear)
                .WithMany(ay => ay.Classes)
                .HasForeignKey(c => c.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Classes_AcademicYears");

            // Sections relationship (one-to-many)
            builder.HasMany(c => c.Sections)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Classes_Sections");

            // Students relationship (one-to-many)
            builder.HasMany(c => c.Students)
                .WithOne(st => st.Class)
                .HasForeignKey(st => st.ClassId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Classes_Students");

            // ========== Indexes for Performance ==========

            builder.HasIndex(c => c.AcademicYearId)
                .HasDatabaseName("IX_Classes_AcademicYear");

            builder.HasIndex(c => c.Grade)
                .HasDatabaseName("IX_Classes_Grade");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Classes_IsActive");

            // Composite indexes
            builder.HasIndex(c => new { c.AcademicYearId, c.IsActive })
                .HasDatabaseName("IX_Classes_AcademicYear_IsActive");

            builder.HasIndex(c => new { c.Grade, c.IsActive })
                .HasDatabaseName("IX_Classes_Grade_IsActive");

            // ========== Audit Fields ==========

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            builder.Property(c => c.CreatedBy)
                .HasMaxLength(100);

            builder.Property(c => c.UpdatedBy)
                .HasMaxLength(100);

            // ========== Concurrency Token ==========
            // ✅ REMOVED - Configured globally in DbContext using PostgreSQL xmin

            // ========== Check Constraints ==========

            builder.HasCheckConstraint(
                "CK_Classes_Grade",
                "\"Grade\" >= 1 AND \"Grade\" <= 12" // PostgreSQL syntax
            );

            builder.HasCheckConstraint(
                "CK_Classes_Capacity",
                "\"Capacity\" >= 0" // PostgreSQL syntax
            );
        }
    }
}
