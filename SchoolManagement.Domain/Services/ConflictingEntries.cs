using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public sealed class ConflictingEntries
    {
        public TimeTableEntry SectionEntry { get; }
        public TimeTableEntry TeacherEntry { get; }
        public TimeTableEntry RoomEntry { get; }

        public ConflictingEntries(
            TimeTableEntry sectionEntry,
            TimeTableEntry teacherEntry,
            TimeTableEntry roomEntry)
        {
            SectionEntry = sectionEntry;
            TeacherEntry = teacherEntry;
            RoomEntry = roomEntry;
        }
    }
}
