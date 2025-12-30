using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class TeacherExperienceUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid TeacherId { get; }
        public decimal TotalExperience { get; }

        public TeacherExperienceUpdatedEvent(Guid teacherId, decimal totalExperience)
        {
            TeacherId = teacherId;
            TotalExperience = totalExperience;
        }
    }
}
