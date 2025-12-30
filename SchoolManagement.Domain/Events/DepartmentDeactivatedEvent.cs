using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class DepartmentDeactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid DepartmentId { get; }
        public string Name { get; }

        public DepartmentDeactivatedEvent(Guid departmentId, string name)
        {
            DepartmentId = departmentId;
            Name = name;
        }
    }
}
