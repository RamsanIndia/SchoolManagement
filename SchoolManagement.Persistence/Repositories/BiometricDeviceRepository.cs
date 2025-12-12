// BiometricDeviceRepository.cs
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
    public class BiometricDeviceRepository : IBiometricDeviceRepository
    {
        private readonly SchoolManagementDbContext _context;

        public BiometricDeviceRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BiometricDevice>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<BiometricDevice>()
                .Where(d => !d.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<BiometricDevice> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<BiometricDevice>()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<BiometricDevice>> GetOfflineDevicesAsync(DateTime cutoffTime, CancellationToken cancellationToken = default)
        {
            return await _context.Set<BiometricDevice>()
                .Where(d => d.IsActive &&
                           !d.IsDeleted &&
                           (!d.IsOnline || d.LastSyncTime < cutoffTime))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<BiometricDevice>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<BiometricDevice>()
                .Where(d => d.IsActive && !d.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(BiometricDevice device, CancellationToken cancellationToken = default)
        {
            await _context.Set<BiometricDevice>().AddAsync(device, cancellationToken);
        }

        public async Task UpdateAsync(BiometricDevice device, CancellationToken cancellationToken = default)
        {
            _context.Set<BiometricDevice>().Update(device);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var device = await GetByIdAsync(id, cancellationToken);
            if (device != null)
            {
                device.MarkAsDeleted(true);
                _context.Set<BiometricDevice>().Update(device);
            }
        }
    }
}
