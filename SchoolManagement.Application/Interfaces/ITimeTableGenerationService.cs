using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ITimeTableGenerationService
    {
        TimeTableGenerationResult GenerateTimeTable(
            Section section,
            List<SectionSubject> subjects,
            List<TimeTableEntry> existingEntries,
            TimeTableGenerationOptions options);
    }
}
