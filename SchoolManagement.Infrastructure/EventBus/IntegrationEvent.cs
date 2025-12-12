using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus
{
    public abstract class IntegrationEvent : IEvent
    {
        protected IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid EventId { get; }
        public DateTime Timestamp { get; }
        public abstract string EventType { get; }

        public DateTime OccurredOn => throw new NotImplementedException();
    }
}
