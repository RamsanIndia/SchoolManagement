using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class TimeTableEntryConfiguration : IEntityTypeConfiguration<TimeTableEntry>
    {
        public void Configure(EntityTypeBuilder<TimeTableEntry> builder)
        {
            builder.ToTable("TimeTableEntries");
            builder.HasKey(tt => tt.Id);

            // Foreign Keys
            builder.Property(tt => tt.SectionId)
                .IsRequired();

            builder.Property(tt => tt.SubjectId)
                .IsRequired();

            builder.Property(tt => tt.TeacherId)
                .IsRequired();

            // Enum Conversion
            builder.Property(tt => tt.DayOfWeek)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(tt => tt.PeriodNumber)
                .IsRequired();

            // Configure TimePeriod Value Object as Owned Entity
            builder.OwnsOne(tt => tt.TimePeriod, tp =>
            {
                tp.Property(t => t.StartTime)
                    .IsRequired()
                    .HasColumnName("StartTime");

                tp.Property(t => t.EndTime)
                    .IsRequired()
                    .HasColumnName("EndTime");
            });

            // Configure RoomNumber Value Object as Owned Entity
            builder.OwnsOne(tt => tt.RoomNumber, rn =>
            {
                rn.Property(r => r.Value)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("RoomNumber");
            });

            // Index - Unique: A section can't have two classes in same day/period
            builder.HasIndex(tt => new { tt.SectionId, tt.DayOfWeek, tt.PeriodNumber })
                .IsUnique()
                .HasDatabaseName("IX_TimeTable_Section_Day_Period");

            // Teacher index
            builder.HasIndex(tt => tt.TeacherId)
                .HasDatabaseName("IX_TimeTable_Teacher");

            // Index to find all periods on a day
            builder.HasIndex(tt => new { tt.DayOfWeek, tt.PeriodNumber })
                .HasDatabaseName("IX_TimeTable_Day_Period");

            // Navigation Property (only Section exists in the entity)
            builder.HasOne(tt => tt.Section)
                .WithMany(s => s.TimeTableEntries)
                .HasForeignKey(tt => tt.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}