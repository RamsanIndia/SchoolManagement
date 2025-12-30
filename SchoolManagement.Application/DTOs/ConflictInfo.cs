using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class ConflictInfo
    {
        public bool HasConflict { get; set; }
        public string ConflictType { get; set; }
        public Guid? ConflictingEntryId { get; set; }
        public string ConflictDetails { get; set; }
        public string TimeSlot { get; set; }

        // Additional context
        public Guid? ConflictingSectionId { get; set; }
        public Guid? ConflictingTeacherId { get; set; }
        public Guid? ConflictingSubjectId { get; set; }
        public string ConflictingRoomNumber { get; set; }
    }
}
