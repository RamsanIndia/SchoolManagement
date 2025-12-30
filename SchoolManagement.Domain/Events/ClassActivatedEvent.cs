using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassActivatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        public Guid ClassId { get; }
        public string Code { get; }
        public string Name { get; }
        public DateTime ActivatedAt { get; }
        public string ActivatedBy { get; }

        public ClassActivatedEvent(
            Guid classId,
            string code,
            string name,
            DateTime activatedAt,
            string activatedBy)
        {
            ClassId = classId;
            Code = code;
            Name = name;
            ActivatedAt = activatedAt;
            ActivatedBy = activatedBy;
        }
    }
}
