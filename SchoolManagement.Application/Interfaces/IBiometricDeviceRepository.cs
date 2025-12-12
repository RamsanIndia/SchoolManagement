// IBiometricDeviceRepository.cs
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IBiometricDeviceRepository
    {
        Task<IEnumerable<BiometricDevice>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BiometricDevice> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<BiometricDevice>> GetOfflineDevicesAsync(DateTime cutoffTime, CancellationToken cancellationToken = default);
        Task<IEnumerable<BiometricDevice>> GetActiveDevicesAsync(CancellationToken cancellationToken = default);
        Task AddAsync(BiometricDevice device, CancellationToken cancellationToken = default);
        Task UpdateAsync(BiometricDevice device, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
