using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }

        // User information
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; }

        // Request context
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        // Timestamp
        public DateTime Timestamp { get; set; }

        // Change tracking (parsed for display)
        public Dictionary<string, object> OldValues { get; set; }
        public Dictionary<string, object> NewValues { get; set; }
        public List<string> ChangedFields { get; set; }

        // Performance metrics
        public long? Duration { get; set; }
        public string Status { get; set; }

        // Human-readable changes
        public List<FieldChangeDto> Changes { get; set; }
    }

    public class FieldChangeDto
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }  // "FirstName" → "First Name"
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }

}
