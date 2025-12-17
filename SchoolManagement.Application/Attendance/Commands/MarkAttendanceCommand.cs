using MediatR;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Attendance.Commands
{
    public class MarkAttendanceCommand : IRequest<Result>
    {
        public Guid StudentId { get; set; }
        public DateTime Timestamp { get; set; }
        public string BiometricData { get; set; }
        public BiometricType BiometricType { get; set; }
        public string DeviceId { get; set; }
        public string MarkedBy { get; set; }
        public string IpAddress { get; set; }
        public string Remarks { get; set; }

        public MarkAttendanceCommand()
        {
            Timestamp = DateTime.Now;
        }
    }
}
