using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ITimeTableRepository : IRepository<TimeTableEntry>
    {
        Task<IEnumerable<TimeTableEntry>> GetBySectionIdAsync(Guid sectionId,CancellationToken cancellationToken);
        Task<IEnumerable<TimeTableEntry>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<TimeTableEntry>> GetByDayAsync(Guid sectionId, DayOfWeek day);
        Task<bool> IsSlotAvailableAsync(Guid sectionId, DayOfWeek day, int periodNumber, CancellationToken cancellationToken, Guid? excludeId = null);
        Task<bool> IsTeacherAvailableAsync(Guid teacherId, DayOfWeek day, int periodNumber, CancellationToken cancellationToken, Guid? excludeId = null);
    }
}
