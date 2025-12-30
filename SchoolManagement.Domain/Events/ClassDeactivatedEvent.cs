using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassDeactivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid ClassId { get; }
        public string Code { get; }
        public string Name { get; }
        public DateTime DeactivatedAt { get; }
        public string DeactivatedBy { get; }

        public ClassDeactivatedEvent(
            Guid classId,
            string code,
            string name,
            DateTime deactivatedAt,
            string deactivatedBy)
        {
            ClassId = classId;
            Code = code;
            Name = name;
            DeactivatedAt = deactivatedAt;
            DeactivatedBy = deactivatedBy;
        }
    }
}
