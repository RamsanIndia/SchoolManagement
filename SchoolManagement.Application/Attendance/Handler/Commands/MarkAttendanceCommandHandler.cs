using MediatR;
using SchoolManagement.Application.Attendance.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Entities = SchoolManagement.Domain.Entities;

namespace SchoolManagement.Application.Features.Attendance.Handlers.Commands
{
    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IBiometricVerificationService _biometricService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public MarkAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IStudentRepository studentRepository,
            IBiometricVerificationService biometricService,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _attendanceRepository = attendanceRepository;
            _studentRepository = studentRepository;
            _biometricService = biometricService;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Verify student exists
                var student = await _studentRepository.GetByIdAsync(request.StudentId);
                if (student == null)
                {
                    return Result.Failure("Student not found");
                }

                // 2. Verify biometric match
                var verificationResult = await _biometricService.VerifyAsync(
                    request.BiometricData, (BiometricType)request.BiometricType);

                if (!verificationResult.IsVerified)
                {
                    return Result.Failure("Biometric verification failed");
                }

                // 3. Check duplicate attendance
                var existingAttendance = await _attendanceRepository.GetTodayAttendanceAsync(
                    request.StudentId, request.Timestamp.Date);

                if (existingAttendance != null)
                {
                    return Result.Failure("Attendance already marked for today");
                }

                // 4. Create attendance entry
                var attendance = new Entities.Attendance(
                    request.StudentId,
                    request.Timestamp.Date,
                    request.Timestamp.TimeOfDay,
                    AttendanceMode.Biometric,
                    request.DeviceId);

                var createdAttendance = await _attendanceRepository.CreateAsync(attendance);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Send notification to parents
                await _notificationService.SendSMSAsync(student.Phone,
                    $"{student.FirstName} has arrived at school at {request.Timestamp:HH:mm}");

                // 6. Return success result
                return Result.Success("Attendance marked successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure("Error marking attendance", ex.Message);
            }
        }
    }
}
