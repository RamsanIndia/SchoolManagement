using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events.Students
{
    public class StudentCreatedEvent : DomainEvent  // ← Inherit from DomainEvent
    {
        public StudentCreatedEvent(Guid studentId, string name, string email)
        {
            StudentId = studentId;
            Name = name;
            Email = email;
        }

        public Guid StudentId { get; }
        public string Name { get; }
        public string Email { get; }
    }
}
