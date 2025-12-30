using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherResignedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string FullName { get; }
        public DateTime ResignationDate { get; }
        public string Reason { get; }

        public TeacherResignedEvent(Guid teacherId, string fullName, DateTime resignationDate,
            string reason)
        {
            TeacherId = teacherId;
            FullName = fullName;
            ResignationDate = resignationDate;
            Reason = reason;
        }
    }
}
