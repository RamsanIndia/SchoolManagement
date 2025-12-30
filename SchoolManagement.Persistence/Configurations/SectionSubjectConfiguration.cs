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

            // ========== Primary Key ==========
            builder.HasKey(ss => ss.Id);

            // ========== Properties ==========

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
                .HasMaxLength(200);

            builder.Property(ss => ss.WeeklyPeriods)
                .IsRequired();

            builder.Property(ss => ss.IsMandatory)
                .IsRequired()
                .HasDefaultValue(true);

            // ========== Audit Fields ==========

            builder.Property(ss => ss.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(ss => ss.UpdatedAt)
                .IsRequired(false);

            builder.Property(ss => ss.CreatedBy)
                .HasMaxLength(100);

            builder.Property(ss => ss.UpdatedBy)
                .HasMaxLength(100);

            // ========== Relationships ==========

            // Section relationship (many-to-one)
            builder.HasOne(ss => ss.Section)
                .WithMany(s => s.SectionSubjects)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SectionSubjects_Sections");

            // Teacher relationship (many-to-one)
            builder.HasOne(ss => ss.Teacher)
                .WithMany(t => t.TeachingAssignments)
                .HasForeignKey(ss => ss.TeacherId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SectionSubjects_Teachers");

            // Note: Subject navigation property is not in your entity, so we don't configure it
            // If you add it later, uncomment this:
            // builder.HasOne(ss => ss.Subject)
            //     .WithMany()
            //     .HasForeignKey(ss => ss.SubjectId)
            //     .OnDelete(DeleteBehavior.Restrict)
            //     .HasConstraintName("FK_SectionSubjects_Subjects");

            // ========== Indexes for Performance ==========

            // Unique constraint: A subject can only be assigned once per section
            builder.HasIndex(ss => new { ss.SectionId, ss.SubjectId })
                .IsUnique()
                .HasDatabaseName("IX_SectionSubjects_Section_Subject");

            // Index on TeacherId for teacher workload queries
            builder.HasIndex(ss => ss.TeacherId)
                .HasDatabaseName("IX_SectionSubjects_Teacher");

            // Index on SubjectId for subject-based queries
            builder.HasIndex(ss => ss.SubjectId)
                .HasDatabaseName("IX_SectionSubjects_Subject");

            // Composite index for section + teacher queries
            builder.HasIndex(ss => new { ss.SectionId, ss.TeacherId })
                .HasDatabaseName("IX_SectionSubjects_Section_Teacher");

            // Index on IsMandatory for filtering mandatory/optional subjects
            builder.HasIndex(ss => ss.IsMandatory)
                .HasDatabaseName("IX_SectionSubjects_IsMandatory");

            // ========== Check Constraints ==========

            // Ensure WeeklyPeriods is within valid range (1-20 periods per week)
            builder.HasCheckConstraint(
                "CK_SectionSubjects_WeeklyPeriods",
                "\"WeeklyPeriods\" > 0 AND \"WeeklyPeriods\" <= 20" // PostgreSQL syntax
            );
        }
    }
}
