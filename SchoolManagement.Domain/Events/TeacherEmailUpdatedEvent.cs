using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherEmailUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string OldEmail { get; }
        public string NewEmail { get; }

        public TeacherEmailUpdatedEvent(Guid teacherId, string oldEmail, string newEmail)
        {
            TeacherId = teacherId;
            OldEmail = oldEmail;
            NewEmail = newEmail;
        }
    }
}
