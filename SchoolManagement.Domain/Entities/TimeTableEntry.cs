using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class TimeTableEntry : BaseEntity
    {
        public Guid SectionId { get; private set; }
        public Guid SubjectId { get; private set; }
        public Guid TeacherId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public int PeriodNumber { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public string RoomNumber { get; private set; }

        // Navigation properties
        public virtual Section Section { get; private set; }

        private TimeTableEntry() { }

        public TimeTableEntry(Guid sectionId, Guid subjectId, Guid teacherId,
             DayOfWeek dayOfWeek, int periodNumber, TimeSpan startTime,
            TimeSpan endTime, string roomNumber)
        {
            SectionId = sectionId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            StartTime = startTime;
            EndTime = endTime;
            RoomNumber = roomNumber;
        }

        public void UpdateDetails(Guid subjectId, Guid teacherId,
            TimeSpan startTime, TimeSpan endTime, string roomNumber)
        {
            SubjectId = subjectId;
            TeacherId = teacherId;
            StartTime = startTime;
            EndTime = endTime;
            RoomNumber = roomNumber;
           
        }
    }
}
