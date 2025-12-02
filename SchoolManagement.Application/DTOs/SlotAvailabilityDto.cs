using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SlotAvailabilityDto
    {
        public bool IsSectionSlotAvailable { get; set; }
        public bool IsTeacherAvailable { get; set; }
        public bool CanSchedule { get; set; }
        public string Message { get; set; }
    }
}
