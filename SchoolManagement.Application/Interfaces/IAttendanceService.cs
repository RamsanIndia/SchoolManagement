using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolManagement.Domain.Entities;


namespace SchoolManagement.Application.Interfaces
{
    public interface IAttendanceService
    {
        // Sync methods
        Task<IEnumerable<BiometricDevice>> GetOfflineDevicesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<OfflineAttendanceRecord>> GetPendingRecordsAsync(Guid deviceId, CancellationToken cancellationToken = default);
        Task ProcessOfflineAttendanceAsync(OfflineAttendanceRecord record, CancellationToken cancellationToken = default);

        // Regular attendance methods
        Task MarkAttendanceAsync(Guid studentId, DateTime date, string status, CancellationToken cancellationToken = default);
        Task<IEnumerable<SchoolManagement.Domain.Entities.Attendance>> GetAttendanceByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IEnumerable<SchoolManagement.Domain.Entities.Attendance>> GetStudentAttendanceAsync(Guid studentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
