using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public record SchoolCreatedEvent(Guid SchoolId, string Name, string Code, SchoolType Type, int Capacity) : IDomainEvent
    {
        public Guid EventId => throw new NotImplementedException();

        public DateTime OccurredOn => throw new NotImplementedException();
    }
}
