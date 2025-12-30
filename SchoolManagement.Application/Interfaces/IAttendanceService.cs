// SchoolManagement.Application/Interfaces/IAttendanceService.cs
using SchoolManagement.Domain.Entities;
using AttendanceEntity = SchoolManagement.Domain.Entities.Attendance; // ✅ Create alias

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
        Task<IEnumerable<AttendanceEntity>> GetAttendanceByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IEnumerable<AttendanceEntity>> GetStudentAttendanceAsync(Guid studentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
