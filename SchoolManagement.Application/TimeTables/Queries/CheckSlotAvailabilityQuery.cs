using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Models;
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
        public DayOfWeek DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
    }
}
