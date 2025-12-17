using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class CreateTimeTableEntryCommand : IRequest<Result<Guid>>
    {
        public Guid SectionId { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int PeriodNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNumber { get; set; }
        
    }
}
