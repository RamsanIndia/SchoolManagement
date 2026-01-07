using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(
            Guid? userId,
            string entityName,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            query = ApplyFilters(query, userId, entityName, startDate, endDate);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetAuditLogsCountAsync(
            Guid? userId,
            string entityName,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            query = ApplyFilters(query, userId, entityName, startDate, endDate);

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetUserActivityAsync(
            Guid userId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetEntityHistoryAsync(
            string entityName,
            string entityId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AuditLog>> GetRecentActivitiesAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        private IQueryable<AuditLog> ApplyFilters(
            IQueryable<AuditLog> query,
            Guid? userId,
            string entityName,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate);

            return query;
        }
    }
}
