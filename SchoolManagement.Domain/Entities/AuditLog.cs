using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid Id { get; private set; }
        public AuditAction Action { get; private set; }
        public string EntityName { get; private set; }
        public string EntityId { get; private set; }
        public Guid? UserId { get; private set; }
        public string UserEmail { get; private set; }
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string OldValues { get; private set; }
        public string NewValues { get; private set; }
        public string ChangedFields { get; private set; }
        public long? Duration { get; private set; }
        public string Status { get; private set; }

        private AuditLog() { }

        public static AuditLog Create(
            AuditAction action,
            string entityName,
            string entityId,
            Guid? userId = null,
            string userEmail = null,
            string ipAddress = null,
            string userAgent = null,
            string oldValues = null,
            string newValues = null,
            string changedFields = null,
            long? duration = null,
            string status = "Success")
        {
            ValidateInputs(action, entityName, entityId);

            return new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityName = NormalizeEntityName(entityName),
                EntityId = entityId,
                UserId = userId,
                UserEmail = userEmail?.Trim(),
                IpAddress = ipAddress?.Trim(),
                UserAgent = TruncateUserAgent(userAgent),
                OldValues = oldValues,
                NewValues = newValues,
                ChangedFields = changedFields,
                Duration = duration,
                Status = status,
                Timestamp = DateTime.UtcNow
            };
        }

        private static void ValidateInputs(AuditAction action, string entityName, string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity name cannot be null or empty.", nameof(entityName));

            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be null or empty.", nameof(entityId));

            if (!Enum.IsDefined(typeof(AuditAction), action))
                throw new ArgumentException("Invalid audit action.", nameof(action));
        }

        private static string NormalizeEntityName(string entityName)
        {
            return entityName.Trim();
        }

        private static string TruncateUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return null;

            const int maxLength = 500;
            return userAgent.Length > maxLength
                ? userAgent.Substring(0, maxLength)
                : userAgent;
        }

        public bool IsUserAction(Guid userId) => UserId == userId;
        public bool IsEntityType(string entityName) =>
            EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase);
        public bool OccurredBetween(DateTime start, DateTime end) =>
            Timestamp >= start && Timestamp <= end;
    }

    
}
