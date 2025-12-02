using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.ToTable("Sections");

            // Primary key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Capacity)
                .IsRequired();

            builder.Property(s => s.RoomNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.CurrentStrength)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(s => s.ClassTeacherId)
                .IsRequired(false); // Explicitly nullable

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(s => new { s.ClassId, s.Name })
                .IsUnique()
                .HasDatabaseName("IX_Sections_Class_Name");

            builder.HasIndex(s => s.ClassTeacherId)
                .HasDatabaseName("IX_Sections_ClassTeacher");

            // Relationships

            // SectionSubjects (1-to-many)
            builder.HasMany(s => s.SectionSubjects)
                .WithOne(ss => ss.Section)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // TimeTableEntries (1-to-many)
            builder.HasMany(s => s.TimeTableEntries)
                .WithOne(tt => tt.Section)
                .HasForeignKey(tt => tt.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
