using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Services
{
    public interface ISlotAvailabilityService
    {
        SlotAvailabilityResult CheckAvailability(
            SlotAvailabilityRequest request,
            TimeTableEntry sectionEntry,
            TimeTableEntry teacherEntry,
            TimeTableEntry roomEntry);
    }
}
