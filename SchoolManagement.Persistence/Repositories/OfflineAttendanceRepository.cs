// OfflineAttendanceRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class OfflineAttendanceRepository : IOfflineAttendanceRepository
    {
        private readonly SchoolManagementDbContext _context;

        public OfflineAttendanceRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OfflineAttendanceRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<OfflineAttendanceRecord>()
                .Where(r => !r.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<OfflineAttendanceRecord> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<OfflineAttendanceRecord>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<OfflineAttendanceRecord>> GetPendingRecordsByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<OfflineAttendanceRecord>()
                .Where(r => r.DeviceId == deviceId &&
                           !r.IsSynced &&
                           !r.IsDeleted)
                .OrderBy(r => r.CheckInTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OfflineAttendanceRecord>> GetSyncedRecordsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<OfflineAttendanceRecord>()
                .Where(r => r.IsSynced && !r.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(OfflineAttendanceRecord record, CancellationToken cancellationToken = default)
        {
            await _context.Set<OfflineAttendanceRecord>().AddAsync(record, cancellationToken);
        }

        public async Task UpdateAsync(OfflineAttendanceRecord record, CancellationToken cancellationToken = default)
        {
            _context.Set<OfflineAttendanceRecord>().Update(record);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var record = await GetByIdAsync(id, cancellationToken);
            if (record != null)
            {
                record.MarkAsDeleted(true);
                _context.Set<OfflineAttendanceRecord>().Update(record);
            }
        }
    }
}
