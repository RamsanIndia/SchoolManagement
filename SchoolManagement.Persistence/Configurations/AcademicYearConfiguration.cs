using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
    {
        public void Configure(EntityTypeBuilder<AcademicYear> builder)
        {
            builder.ToTable("AcademicYears");

            builder.HasKey(ay => ay.Id);

            builder.Property(ay => ay.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(ay => ay.Name)
                .IsUnique()
                .HasDatabaseName("IX_AcademicYears_Name");

            builder.Property(ay => ay.StartYear)
                .IsRequired();

            builder.Property(ay => ay.EndYear)
                .IsRequired();

            builder.Property(ay => ay.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(ay => ay.EndDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(ay => ay.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ay => ay.IsCurrent)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasMany(ay => ay.Classes)
                .WithOne(c => c.AcademicYear)
                .HasForeignKey(c => c.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AcademicYears_Classes");

            // Indexes
            builder.HasIndex(ay => ay.IsActive)
                .HasDatabaseName("IX_AcademicYears_IsActive");

            builder.HasIndex(ay => ay.IsCurrent)
                .HasDatabaseName("IX_AcademicYears_IsCurrent");

            builder.HasIndex(ay => new { ay.StartYear, ay.EndYear })
                .IsUnique()
                .HasDatabaseName("IX_AcademicYears_Years");

            // Audit fields
            builder.Property(ay => ay.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(ay => ay.UpdatedAt);

            builder.Property(ay => ay.CreatedBy)
                .HasMaxLength(100);

            builder.Property(ay => ay.UpdatedBy)
                .HasMaxLength(100);

            // Check constraints
            builder.HasCheckConstraint(
                "CK_AcademicYears_Dates",
                "\"StartDate\" < \"EndDate\"" // PostgreSQL syntax
            );

            builder.HasCheckConstraint(
                "CK_AcademicYears_Years",
                "\"EndYear\" = \"StartYear\" + 1" // PostgreSQL syntax
            );
        }
    }
}
