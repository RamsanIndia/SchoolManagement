using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class DepartmentCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid DepartmentId { get; }
        public string Name { get; }
        public string Code { get; }

        public DepartmentCreatedEvent(Guid departmentId, string name, string code)
        {
            DepartmentId = departmentId;
            Name = name;
            Code = code;
        }
    }
}
