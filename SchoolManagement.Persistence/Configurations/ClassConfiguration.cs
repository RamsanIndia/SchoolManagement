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

            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Grade)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.AcademicYearId)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("IX_Classes_ClassCode");

            builder.HasIndex(c => c.AcademicYearId)
                .HasDatabaseName("IX_Classes_AcademicYear");

            builder.HasIndex(c => c.Grade)
                .HasDatabaseName("IX_Classes_Grade");

            // Relationships

            // Class -> Sections (1-to-many)
            builder.HasMany(c => c.Sections)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
