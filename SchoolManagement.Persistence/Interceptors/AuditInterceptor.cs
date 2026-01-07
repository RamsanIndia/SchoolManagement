using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities(eventData.Context);
            await CreateAuditLogsAsync(eventData.Context, cancellationToken);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted);

            var userId = _currentUserService.UserId;
            var userName = _currentUserService.FullName ?? _currentUserService.Username;
            var ipAddress = _currentUserService.IpAddress;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userName;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedIP = ipAddress;
                }

                if (entry.State == EntityState.Modified)
                {
                    // Prevent modification of creation audit fields
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedIP)).IsModified = false;

                    entry.Entity.UpdatedBy = userName;
                    
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedIP = ipAddress;
                }

                if (entry.State == EntityState.Deleted)
                {
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.MarkAsDeleted(userName);
                }
            }
        }

        private async Task CreateAuditLogsAsync(DbContext context, CancellationToken cancellationToken)
        {
            if (context == null) return;

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // Skip audit logs and unchanged entities
                if (entry.Entity is AuditLog ||
                    entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var auditLog = CreateAuditLog(entry);
                if (auditLog != null)
                {
                    auditEntries.Add(auditLog);
                }
            }

            if (auditEntries.Any())
            {
                context.Set<AuditLog>().AddRange(auditEntries);
            }
        }

        private AuditLog CreateAuditLog(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = GetPrimaryKeyValue(entry);
            var action = GetAuditAction(entry.State);

            if (action == null)
                return null;

            var oldValues = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                ? SerializeObject(GetOriginalValues(entry))
                : null;

            var newValues = entry.State == EntityState.Added || entry.State == EntityState.Modified
                ? SerializeObject(GetCurrentValues(entry))
                : null;

            var changedFields = entry.State == EntityState.Modified
                ? string.Join(", ", GetChangedFields(entry))
                : null;

            return AuditLog.Create(
                action: action.Value,
                entityName: entityName,
                entityId: entityId,
                userId: _currentUserService.UserId,
                userEmail: _currentUserService.Email,
                ipAddress: _currentUserService.IpAddress,
                userAgent: _currentUserService.UserAgent,
                oldValues: oldValues,
                newValues: newValues,
                changedFields: changedFields);
        }

        private AuditAction? GetAuditAction(EntityState state)
        {
            return state switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => null
            };
        }

        private string GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyValues = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .Select(p => p.CurrentValue?.ToString())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();

            return keyValues.Any() ? string.Join(", ", keyValues) : "Unknown";
        }

        private Dictionary<string, object> GetOriginalValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.Properties
                .Where(p => !IsNavigationProperty(p) && !IsSystemProperty(p))
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => p.OriginalValue ?? "NULL");
        }

        private Dictionary<string, object> GetCurrentValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.Properties
                .Where(p => !IsNavigationProperty(p) && !IsSystemProperty(p))
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => p.CurrentValue ?? "NULL");
        }

        private List<string> GetChangedFields(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.Properties
                .Where(p => p.IsModified &&
                           !IsNavigationProperty(p) &&
                           !IsSystemProperty(p))
                .Select(p => p.Metadata.Name)
                .ToList();
        }

        private bool IsNavigationProperty(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry property)
        {
            return property.Metadata.IsForeignKey();
        }

        private bool IsSystemProperty(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry property)
        {
            // Exclude system properties from detailed audit logs
            var systemProperties = new[]
            {
                nameof(BaseEntity.RowVersion),
                nameof(BaseEntity.DomainEvents)
            };

            return systemProperties.Contains(property.Metadata.Name);
        }

        private string SerializeObject(Dictionary<string, object> values)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                return JsonSerializer.Serialize(values, options);
            }
            catch (Exception)
            {
                return "Unable to serialize";
            }
        }
    }
}
