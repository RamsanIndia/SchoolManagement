// Application/Services/AttendanceService.cs
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using AttendanceEntity = SchoolManagement.Domain.Entities.Attendance;

namespace SchoolManagement.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IBiometricDeviceRepository _biometricDeviceRepository;
        private readonly IOfflineAttendanceRepository _offlineAttendanceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IStudentRepository studentRepository,
            IBiometricDeviceRepository biometricDeviceRepository,
            IOfflineAttendanceRepository offlineAttendanceRepository,
            IUnitOfWork unitOfWork,
            ILogger<AttendanceService> logger)
        {
            _attendanceRepository = attendanceRepository;
            _studentRepository = studentRepository;
            _biometricDeviceRepository = biometricDeviceRepository;
            _offlineAttendanceRepository = offlineAttendanceRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<BiometricDevice>> GetOfflineDevicesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
                return await _biometricDeviceRepository.GetOfflineDevicesAsync(cutoffTime, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching offline devices");
                return Enumerable.Empty<BiometricDevice>();
            }
        }

        public async Task<IEnumerable<OfflineAttendanceRecord>> GetPendingRecordsAsync(
            Guid deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _offlineAttendanceRepository.GetPendingRecordsByDeviceAsync(deviceId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending records for device {DeviceId}", deviceId);
                return Enumerable.Empty<OfflineAttendanceRecord>();
            }
        }

        public async Task ProcessOfflineAttendanceAsync(
            OfflineAttendanceRecord record,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var student = await _studentRepository.GetByCodeAsync(record.StudentCode, cancellationToken);

                if (student == null)
                {
                    _logger.LogWarning("Student not found for code {StudentCode}", record.StudentCode);
                    record.MarkSyncFailed("Student not found", "SYSTEM");
                    await _offlineAttendanceRepository.UpdateAsync(record, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }

                var existingAttendance = await _attendanceRepository.GetByStudentAndDateAsync(
                    student.Id,
                    record.CheckInTime.Date,
                    cancellationToken);

                if (existingAttendance != null)
                {
                    _logger.LogInformation("Attendance already exists for student {StudentId} on {Date}",
                        student.Id, record.CheckInTime.Date);

                    record.MarkAsSynced("SYSTEM");
                    await _offlineAttendanceRepository.UpdateAsync(record, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }

                // Create attendance using factory method
                var attendance = AttendanceEntity.Create(
                    studentId: student.Id,
                    date: record.CheckInTime.Date,
                    status: AttendanceStatus.Present,
                    createdBy: "SYSTEM",
                    createdIp: "Device",
                    checkInTime: record.CheckInTime,
                    isFromBiometric: true,
                    biometricDeviceId: record.DeviceId,
                    remarks: $"Synced from device {record.DeviceId}");

                await _attendanceRepository.AddAsync(attendance, cancellationToken);

                record.MarkAsSynced("SYSTEM");
                await _offlineAttendanceRepository.UpdateAsync(record, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Processed offline attendance for student {StudentId}", student.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing offline attendance record {RecordId}", record.Id);

                record.MarkSyncFailed(ex.Message, "SYSTEM");
                await _offlineAttendanceRepository.UpdateAsync(record, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw;
            }
        }

        public async Task MarkAttendanceAsync(
            Guid studentId,
            DateTime date,
            string status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var existingAttendance = await _attendanceRepository.GetByStudentAndDateAsync(
                    studentId,
                    date.Date,
                    cancellationToken);

                if (existingAttendance != null)
                {
                    _logger.LogWarning("Attendance already marked for student {StudentId} on {Date}",
                        studentId, date);
                    return;
                }

                // Create attendance using factory method
                var attendance = AttendanceEntity.Create(
                    studentId: studentId,
                    date: date.Date,
                    status: AttendanceStatus.Present,
                    createdBy: "SYSTEM",
                    createdIp: "Manual",
                    checkInTime: status == "Present" ? DateTime.Now : (DateTime?)null,
                    isFromBiometric: false,
                    biometricDeviceId: null,
                    remarks: "Manually marked");

                await _attendanceRepository.AddAsync(attendance, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Marked attendance for student {StudentId} as {Status}",
                    studentId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<AttendanceEntity>> GetAttendanceByDateAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _attendanceRepository.GetByDateAsync(date.Date, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance for date {Date}", date);
                return Enumerable.Empty<AttendanceEntity>();
            }
        }

        public async Task<IEnumerable<AttendanceEntity>> GetStudentAttendanceAsync(
            Guid studentId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _attendanceRepository.GetStudentAttendanceAsync(
                    studentId,
                    startDate.Date,
                    endDate.Date,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student attendance for {StudentId}", studentId);
                return Enumerable.Empty<AttendanceEntity>();
            }
        }
    }
}
