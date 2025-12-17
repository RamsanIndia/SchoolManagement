using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Persistence.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> entity)
        {
            entity.ToTable("Attendances");

            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Attendances)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.CheckInTime)
                  .IsRequired();

            entity.Property(e => e.CheckOutTime);

            entity.Property(e => e.Status)
                  .IsRequired();

            entity.Property(e => e.Remarks)
                  .HasMaxLength(500);

            entity.HasIndex(e => new { e.StudentId, e.Date })
                  .IsUnique();

        }
    }
}