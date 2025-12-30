using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public string FullName { get; }
        public string Email { get; }
        public string EmployeeId { get; }
        public DateTime DateOfJoining { get; }

        public TeacherCreatedEvent(Guid teacherId, string fullName, string email,
            string employeeId, DateTime dateOfJoining)
        {
            TeacherId = teacherId;
            FullName = fullName;
            Email = email;
            EmployeeId = employeeId;
            DateOfJoining = dateOfJoining;
        }
    }
}
