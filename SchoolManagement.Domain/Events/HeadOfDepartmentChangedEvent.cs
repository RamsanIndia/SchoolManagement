using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class HeadOfDepartmentChangedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid DepartmentId { get; }
        public string DepartmentName { get; }
        public Guid PreviousHeadId { get; }
        public Guid NewHeadId { get; }

        public HeadOfDepartmentChangedEvent(
            Guid departmentId,
            string departmentName,
            Guid previousHeadId,
            Guid newHeadId)
        {
            DepartmentId = departmentId;
            DepartmentName = departmentName;
            PreviousHeadId = previousHeadId;
            NewHeadId = newHeadId;
        }
    }
}
