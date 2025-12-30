using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherPersonalDetailsUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string FullName { get; }
        public string PhoneNumber { get; }

        public TeacherPersonalDetailsUpdatedEvent(Guid teacherId, string fullName, string phoneNumber)
        {
            TeacherId = teacherId;
            FullName = fullName;
            PhoneNumber = phoneNumber;
        }
    }
}
