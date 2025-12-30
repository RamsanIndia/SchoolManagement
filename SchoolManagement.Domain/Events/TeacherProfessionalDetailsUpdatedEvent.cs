using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherProfessionalDetailsUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string Qualification { get; }
        public decimal Experience { get; }
        public Guid? PreviousDepartmentId { get; }
        public Guid? NewDepartmentId { get; }

        public TeacherProfessionalDetailsUpdatedEvent(Guid teacherId, string qualification,
            decimal experience, Guid? previousDepartmentId, Guid? newDepartmentId)
        {
            TeacherId = teacherId;
            Qualification = qualification;
            Experience = experience;
            PreviousDepartmentId = previousDepartmentId;
            NewDepartmentId = newDepartmentId;
        }
    }
}
