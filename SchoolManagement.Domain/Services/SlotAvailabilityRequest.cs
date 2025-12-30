using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public sealed class SlotAvailabilityRequest
    {
        public Guid SectionId { get; }
        public Guid TeacherId { get; }
        public string RoomNumber { get; }
        public DayOfWeek DayOfWeek { get; }
        public int PeriodNumber { get; }

        public SlotAvailabilityRequest(
            Guid sectionId,
            Guid teacherId,
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber)
        {
            SectionId = sectionId;
            TeacherId = teacherId;
            RoomNumber = roomNumber;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
        }
    }
}
