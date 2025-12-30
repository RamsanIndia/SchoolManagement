using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Mappers
{
    public sealed class SlotAvailabilityMapper : ISlotAvailabilityMapper
    {
        public SlotAvailabilityDto MapToDto(
            CheckSlotAvailabilityQuery query,
            SlotAvailabilityResult result)
        {
            var dto = new SlotAvailabilityDto
            {
                SectionId = query.SectionId,
                TeacherId = query.TeacherId,
                RoomNumber = query.RoomNumber,
                DayOfWeek = query.DayOfWeek.ToString(),
                PeriodNumber = query.PeriodNumber,
                IsAvailable = result.IsAvailable,
                CheckedAt = DateTime.UtcNow
            };

            // Map conflicts to DTO
            foreach (var conflict in result.Conflicts)
            {
                var conflictInfo = MapConflictToInfo(conflict);

                switch (conflict.Type)
                {
                    case ConflictType.Section:
                        dto.SectionConflict = conflictInfo;
                        break;
                    case ConflictType.Teacher:
                        dto.TeacherConflict = conflictInfo;
                        break;
                    case ConflictType.Room:
                        dto.RoomConflict = conflictInfo;
                        break;
                }
            }

            // Build conflicts summary list
            dto.Conflicts = result.Conflicts
                .Select(c => $"{c.Type}: {c.Description}")
                .ToList();

            return dto;
        }

        private static ConflictInfo MapConflictToInfo(ConflictDetail conflict)
        {
            return new ConflictInfo
            {
                HasConflict = true,
                ConflictingEntryId = conflict.ConflictingEntryId,
                ConflictType = conflict.Type.ToString(),
                ConflictDetails = conflict.Description,
                ConflictingSectionId = conflict.ConflictingSectionId,
                ConflictingTeacherId = conflict.ConflictingTeacherId,
                ConflictingSubjectId = conflict.ConflictingSubjectId,
                ConflictingRoomNumber = conflict.ConflictingRoomNumber,
                TimeSlot = $"{conflict.StartTime:hh\\:mm} - {conflict.EndTime:hh\\:mm}"
            };
        }
    }
}
