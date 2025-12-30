using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SlotAvailabilityDto
    {
        public Guid SectionId { get; set; }
        public Guid TeacherId { get; set; }
        public string RoomNumber { get; set; }
        public string DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CheckedAt { get; set; }

        public ConflictInfo SectionConflict { get; set; }
        public ConflictInfo TeacherConflict { get; set; }
        public ConflictInfo RoomConflict { get; set; }

        public List<string> Conflicts { get; set; } = new();

        // Legacy properties for backward compatibility
        public bool IsSectionSlotAvailable => SectionConflict == null;
        public bool IsTeacherAvailable => TeacherConflict == null;
        public bool IsRoomAvailable => RoomConflict == null;
        public bool CanSchedule => IsAvailable;
    }
}
