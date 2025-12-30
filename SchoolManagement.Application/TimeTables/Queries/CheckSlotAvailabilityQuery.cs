using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Queries
{
    public class CheckSlotAvailabilityQuery : IRequest<Result<SlotAvailabilityDto>>
    {
        public Guid SectionId { get; set; }
        public Guid TeacherId { get; set; }
        public string RoomNumber { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
        public Guid? ExcludeEntryId { get; set; } // For update scenarios

        public CheckSlotAvailabilityQuery()
        {
        }

        public CheckSlotAvailabilityQuery(
            Guid sectionId,
            Guid teacherId,
            string roomNumber,
            DayOfWeek dayOfWeek,
            int periodNumber,
            Guid? excludeEntryId = null)
        {
            SectionId = sectionId;
            TeacherId = teacherId;
            RoomNumber = roomNumber;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            ExcludeEntryId = excludeEntryId;
        }
    }
}
