using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class SectionSubjectConfiguration : IEntityTypeConfiguration<SectionSubject>
    {
        public void Configure(EntityTypeBuilder<SectionSubject> builder)
        {
            builder.ToTable("SectionSubjects");

            // Primary Key
            builder.HasKey(ss => ss.Id);

            // Properties
            builder.Property(ss => ss.SectionId)
                .IsRequired();

            builder.Property(ss => ss.SubjectId)
                .IsRequired();

            builder.Property(ss => ss.SubjectName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ss => ss.SubjectCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(ss => ss.TeacherId)
                .IsRequired();

            builder.Property(ss => ss.TeacherName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ss => ss.WeeklyPeriods)
                .IsRequired();

            builder.Property(ss => ss.IsMandatory)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ss => ss.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(ss => new { ss.SectionId, ss.SubjectId })
                .IsUnique()
                .HasDatabaseName("IX_SectionSubjects_Section_Subject");

            builder.HasIndex(ss => ss.TeacherId)
                .HasDatabaseName("IX_SectionSubjects_Teacher");

            // Navigation
            builder.HasOne(ss => ss.Section)
                .WithMany(s => s.SectionSubjects)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
