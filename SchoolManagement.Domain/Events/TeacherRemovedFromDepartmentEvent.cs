using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherRemovedFromDepartmentEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public Guid DepartmentId { get; }

        public TeacherRemovedFromDepartmentEvent(Guid teacherId, Guid departmentId)
        {
            TeacherId = teacherId;
            DepartmentId = departmentId;
        }
    }
}
