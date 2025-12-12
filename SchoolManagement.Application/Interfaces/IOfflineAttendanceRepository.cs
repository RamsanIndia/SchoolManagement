// IOfflineAttendanceRepository.cs
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IOfflineAttendanceRepository
    {
        Task<IEnumerable<OfflineAttendanceRecord>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OfflineAttendanceRecord> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OfflineAttendanceRecord>> GetPendingRecordsByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<OfflineAttendanceRecord>> GetSyncedRecordsAsync(CancellationToken cancellationToken = default);
        Task AddAsync(OfflineAttendanceRecord record, CancellationToken cancellationToken = default);
        Task UpdateAsync(OfflineAttendanceRecord record, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
