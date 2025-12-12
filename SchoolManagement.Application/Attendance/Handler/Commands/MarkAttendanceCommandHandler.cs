// Application/Features/Attendance/Handlers/Commands/MarkAttendanceCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Attendance.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Features.Attendance.Handlers.Commands
{
    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IBiometricVerificationService _biometricService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkAttendanceCommandHandler> _logger;

        public MarkAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IStudentRepository studentRepository,
            IBiometricVerificationService biometricService,
            IUnitOfWork unitOfWork,
            ILogger<MarkAttendanceCommandHandler> logger)
        {
            _attendanceRepository = attendanceRepository;
            _studentRepository = studentRepository;
            _biometricService = biometricService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing attendance marking for student {StudentId}", request.StudentId);

                // 1. Verify student exists and is active
                var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
                if (student == null)
                {
                    _logger.LogWarning("Student not found: {StudentId}", request.StudentId);
                    return Result.Failure("Student not found");
                }

                if (!student.IsActive)
                {
                    _logger.LogWarning("Attempt to mark attendance for inactive student: {StudentId}", request.StudentId);
                    return Result.Failure("Student is not active");
                }

                // 2. Verify biometric match if biometric data provided
                if (!string.IsNullOrEmpty(request.BiometricData))
                {
                    var verificationResult = await _biometricService.VerifyAsync(
                        request.BiometricData,
                        request.BiometricType);

                    if (!verificationResult.IsVerified)
                    {
                        _logger.LogWarning(
                            "Biometric verification failed for student {StudentId}. Confidence: {Confidence}",
                            request.StudentId,
                            verificationResult.ConfidenceScore);
                        return Result.Failure("Biometric verification failed");
                    }

                    _logger.LogInformation(
                        "Biometric verified for student {StudentId}. Confidence: {Confidence}",
                        request.StudentId,
                        verificationResult.ConfidenceScore);
                }

                // 3. Check duplicate attendance
                var existingAttendance = await _attendanceRepository.GetByStudentAndDateAsync(
                    request.StudentId,
                    request.Timestamp.Date,
                    cancellationToken);

                if (existingAttendance != null)
                {
                    _logger.LogInformation(
                        "Attendance already exists for student {StudentId} on {Date}",
                        request.StudentId,
                        request.Timestamp.Date);
                    return Result.Failure("Attendance already marked for today");
                }

                // 4. Parse device ID
                Guid? deviceId = null;
                if (!string.IsNullOrEmpty(request.DeviceId) && Guid.TryParse(request.DeviceId, out var parsedDeviceId))
                {
                    deviceId = parsedDeviceId;
                }

                // 5. Determine attendance status
                var status = DetermineAttendanceStatus(request.Timestamp);

                // 6. Create attendance using factory method
                var attendance = Domain.Entities.Attendance.Create(
                    studentId: request.StudentId,
                    date: request.Timestamp.Date,
                    status: status,
                    createdBy: request.MarkedBy ?? "SYSTEM",
                    createdIp: request.IpAddress ?? "Device",
                    checkInTime: request.Timestamp,
                    isFromBiometric: !string.IsNullOrEmpty(request.BiometricData),
                    biometricDeviceId: deviceId,
                    remarks: request.Remarks);

                // 7. Save attendance
                await _attendanceRepository.AddAsync(attendance, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Domain events are automatically dispatched by UnitOfWork
                // Events: AttendanceMarkedEvent (which triggers notification to parents)

                _logger.LogInformation(
                    "Successfully marked attendance for student {StudentId} at {Time}. Status: {Status}",
                    request.StudentId,
                    request.Timestamp,
                    status);

                return Result.Success("Attendance marked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error marking attendance for student {StudentId}",
                    request.StudentId);
                return Result.Failure("Error marking attendance. Please try again.");
            }
        }

        private AttendanceStatus DetermineAttendanceStatus(DateTime timestamp)
        {
            // Define school timing (you can move this to configuration)
            var schoolStartTime = new TimeSpan(8, 0, 0); // 8:00 AM
            var lateThreshold = new TimeSpan(8, 15, 0);  // 8:15 AM

            var checkInTime = timestamp.TimeOfDay;

            if (checkInTime <= lateThreshold)
            {
                return AttendanceStatus.Present;
            }
            else
            {
                return AttendanceStatus.Late;
            }
        }
    }
}
