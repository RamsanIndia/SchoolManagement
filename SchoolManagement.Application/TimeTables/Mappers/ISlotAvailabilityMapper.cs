using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Mappers
{
    public interface ISlotAvailabilityMapper
    {
        SlotAvailabilityDto MapToDto(
            CheckSlotAvailabilityQuery query,
            SlotAvailabilityResult result);
    }
}
