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

            // ========== Primary Key ==========
            builder.HasKey(s => s.Id);

            // ========== Primitive Properties ==========

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.ClassId)
                .IsRequired();

            builder.Property(s => s.ClassTeacherId)
                .IsRequired(false); // Nullable - not all sections may have a class teacher

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ========== Value Objects Configuration ==========

            // Configure RoomNumber as owned type
            builder.OwnsOne(s => s.RoomNumber, room =>
            {
                room.Property(r => r.Value)
                    .HasColumnName("RoomNumber")
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // Configure SectionCapacity as owned type
            builder.OwnsOne(s => s.Capacity, capacity =>
            {
                capacity.Property(c => c.MaxCapacity)
                    .HasColumnName("MaxCapacity")
                    .IsRequired();

                capacity.Property(c => c.CurrentStrength)
                    .HasColumnName("CurrentStrength")
                    .IsRequired()
                    .HasDefaultValue(0);

                // Methods are not mapped by EF Core - no need to ignore them
                // Remove the Ignore() calls for methods HasAvailableSeats() and AvailableSeats()
            });

            // ========== Relationships ==========

            // Class relationship (many-to-one)
            builder.HasOne(s => s.Class)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sections_Classes");

            // Class Teacher relationship (many-to-one with Teacher)
            // Teacher can be class teacher for multiple sections
            builder.HasOne<Teacher>()
                .WithMany(t => t.ClassTeacherSections)
                .HasForeignKey(s => s.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Sections_Teachers_ClassTeacher");

            // Students relationship (one-to-many)
            builder.HasMany(s => s.Students)
                .WithOne(st => st.Section)
                .HasForeignKey(st => st.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sections_Students");

            // SectionSubjects relationship (one-to-many)
            builder.HasMany(s => s.SectionSubjects)
                .WithOne(ss => ss.Section)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Sections_SectionSubjects");

            // TimeTableEntries relationship (one-to-many)
            builder.HasMany(s => s.TimeTableEntries)
                .WithOne(tt => tt.Section)
                .HasForeignKey(tt => tt.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Sections_TimeTableEntries");

            // ========== Indexes for Performance ==========

            // Unique constraint on Class + Section Name combination
            builder.HasIndex(s => new { s.ClassId, s.Name })
                .IsUnique()
                .HasDatabaseName("IX_Sections_Class_Name");

            // Index on ClassTeacherId for quick lookups
            builder.HasIndex(s => s.ClassTeacherId)
                .HasDatabaseName("IX_Sections_ClassTeacher");

            // Index on IsActive for filtering active sections
            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Sections_IsActive");

            // Composite index for class filtering with active status
            builder.HasIndex(s => new { s.ClassId, s.IsActive })
                .HasDatabaseName("IX_Sections_Class_IsActive");

            // ========== Audit Fields ==========

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

            builder.Property(s => s.UpdatedAt)
                .IsRequired(false);

            builder.Property(s => s.CreatedBy)
                .HasMaxLength(100);

            builder.Property(s => s.UpdatedBy)
                .HasMaxLength(100);

            // ========== Check Constraints ==========

            // Ensure CurrentStrength doesn't exceed MaxCapacity
            builder.HasCheckConstraint(
                "CK_Sections_Capacity",
                "\"CurrentStrength\" <= \"MaxCapacity\"" // PostgreSQL uses double quotes for identifiers
            );

            // Ensure MaxCapacity is positive
            builder.HasCheckConstraint(
                "CK_Sections_MaxCapacity",
                "\"MaxCapacity\" > 0"
            );

            // Ensure CurrentStrength is non-negative
            builder.HasCheckConstraint(
                "CK_Sections_CurrentStrength",
                "\"CurrentStrength\" >= 0"
            );
        }
    }
}
