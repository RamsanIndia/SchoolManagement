using MediatR;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Attendance.Commands
{
    public class ManualAttendanceCommand : IRequest<Result>
    {
        public Guid StudentId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }
        public Guid MarkedBy { get; set; }
    }
}
