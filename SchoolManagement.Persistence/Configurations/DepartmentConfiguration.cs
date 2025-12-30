using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(d => d.Name)
                .IsUnique();

            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(d => d.Code)
                .IsUnique();

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Head of Department relationship (self-referencing to Teacher)
            builder.HasOne(d => d.HeadOfDepartment)
                .WithMany() // Teacher doesn't need a back-reference
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Teachers relationship
            builder.HasMany(d => d.Teachers)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Audit fields
            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt);

            builder.Property(d => d.CreatedBy)
                .HasMaxLength(100);

            builder.Property(d => d.UpdatedBy)
                .HasMaxLength(100);
        }
    }
}
