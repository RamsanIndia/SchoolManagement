using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public record SchoolUpdatedEvent(Guid SchoolId, string Name, string Code) : IDomainEvent
    {
        public Guid EventId => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }
}
