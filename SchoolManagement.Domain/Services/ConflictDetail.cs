using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public sealed class ConflictDetail
    {
        public ConflictType Type { get; }
        public Guid ConflictingEntryId { get; }
        public string Description { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public Guid? ConflictingSectionId { get; }
        public Guid? ConflictingTeacherId { get; }
        public Guid? ConflictingSubjectId { get; }
        public string ConflictingRoomNumber { get; }

        public ConflictDetail(
            ConflictType type,
            Guid conflictingEntryId,
            string description,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? conflictingSectionId = null,
            Guid? conflictingTeacherId = null,
            Guid? conflictingSubjectId = null,
            string conflictingRoomNumber = null)
        {
            Type = type;
            ConflictingEntryId = conflictingEntryId;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            StartTime = startTime;
            EndTime = endTime;
            ConflictingSectionId = conflictingSectionId;
            ConflictingTeacherId = conflictingTeacherId;
            ConflictingSubjectId = conflictingSubjectId;
            ConflictingRoomNumber = conflictingRoomNumber;
        }
    }
}
