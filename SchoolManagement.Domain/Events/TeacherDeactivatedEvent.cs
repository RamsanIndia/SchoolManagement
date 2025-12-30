using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherDeactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string DeactivatedBy { get; }
        public DateTime LeavingDate { get; }

        public TeacherDeactivatedEvent(Guid teacherId, string deactivatedBy, DateTime leavingDate)
        {
            TeacherId = teacherId;
            DeactivatedBy = deactivatedBy;
            LeavingDate = leavingDate;
        }
    }
}
