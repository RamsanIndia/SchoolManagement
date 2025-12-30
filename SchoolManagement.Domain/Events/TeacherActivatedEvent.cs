using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherActivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string ActivatedBy { get; }

        public TeacherActivatedEvent(Guid teacherId, string activatedBy)
        {
            TeacherId = teacherId;
            ActivatedBy = activatedBy;
        }
    }
}
