using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Events
{
    public class ClassCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid ClassId { get; }
        public string Code { get; }
        public string Name { get; }
        public int Grade { get; }
        public Guid AcademicYearId { get; }
        public DateTime CreatedAt { get; }
        public string CreatedBy { get; }

        public ClassCreatedEvent(
            Guid classId,
            string code,
            string name,
            int grade,
            Guid academicYearId,
            DateTime createdAt,
            string createdBy)
        {
            ClassId = classId;
            Code = code;
            Name = name;
            Grade = grade;
            AcademicYearId = academicYearId;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
        }
    }
}
