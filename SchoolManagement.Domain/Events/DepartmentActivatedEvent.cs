using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class DepartmentActivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid DepartmentId { get; }
        public string Name { get; }

        public DepartmentActivatedEvent(Guid departmentId, string name)
        {
            DepartmentId = departmentId;
            Name = name;
        }
    }
}
