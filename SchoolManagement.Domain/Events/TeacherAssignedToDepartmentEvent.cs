using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherAssignedToDepartmentEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public Guid? PreviousDepartmentId { get; }
        public Guid NewDepartmentId { get; }

        public TeacherAssignedToDepartmentEvent(Guid teacherId, Guid? previousDepartmentId,
            Guid newDepartmentId)
        {
            TeacherId = teacherId;
            PreviousDepartmentId = previousDepartmentId;
            NewDepartmentId = newDepartmentId;
        }
    }

}
