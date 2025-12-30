using MediatR;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class UpdateTimeTableEntryCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public Guid TeacherId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNumber { get; set; }

        // Constructor for easy instantiation
        public UpdateTimeTableEntryCommand()
        {
        }

        public UpdateTimeTableEntryCommand(
            Guid id,
            Guid subjectId,
            Guid teacherId,
            TimeSpan startTime,
            TimeSpan endTime,
            string roomNumber)
        {
            Id = id;
            SubjectId = subjectId;
            TeacherId = teacherId;
            StartTime = startTime;
            EndTime = endTime;
            RoomNumber = roomNumber;
        }
    }
}
