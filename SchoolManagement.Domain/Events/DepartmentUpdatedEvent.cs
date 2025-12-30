using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class DepartmentUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid DepartmentId { get; }
        public string PreviousName { get; }
        public string NewName { get; }
        public string PreviousCode { get; }
        public string NewCode { get; }

        public DepartmentUpdatedEvent(
            Guid departmentId,
            string previousName,
            string newName,
            string previousCode,
            string newCode)
        {
            DepartmentId = departmentId;
            PreviousName = previousName;
            NewName = newName;
            PreviousCode = previousCode;
            NewCode = newCode;
        }
    }
}
