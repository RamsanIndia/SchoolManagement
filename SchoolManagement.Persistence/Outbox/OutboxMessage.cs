using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string CorrelationId { get; set; }
        public string CausationId { get; set; }
        public string Source { get; set; }
        public string Metadata { get; set; }
        public int? RetryCount { get; set; }
        public string? Error { get; set; }
        public byte[] RowVersion { get; set; }

        public bool IsProcessed => ProcessedAt.HasValue;
    }
}
