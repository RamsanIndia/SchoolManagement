using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<List<AuditLog>> GetAuditLogsAsync(
            Guid? userId,
            string entityName,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<int> GetAuditLogsCountAsync(
            Guid? userId,
            string entityName,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetUserActivityAsync(
            Guid userId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetEntityHistoryAsync(
            string entityName,
            string entityId,
            CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetRecentActivitiesAsync(
            int count,
            CancellationToken cancellationToken = default);
    }
}
