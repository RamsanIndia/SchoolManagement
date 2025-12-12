using Entities = SchoolManagement.Domain.Entities;
using SchoolManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Entities.Attendance>> GetAllAsync();
        Task<Entities.Attendance> CreateAsync(Entities.Attendance attendance, CancellationToken cancellationToken);
        Task<IEnumerable<Entities.Attendance>> GetStudentAttendanceAsync(Guid studentId, DateTime fromDate, DateTime toDate,CancellationToken cancellationToken);
        Task<IEnumerable<Entities.Attendance>> GetClassAttendanceAsync(Guid classId, DateTime date, CancellationToken cancellationToken);
        Task<Entities.Attendance> GetTodayAttendanceAsync(Guid studentId, DateTime date, CancellationToken cancellationToken);
        Task<AttendanceStatistics> GetAttendanceStatisticsAsync(Guid studentId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken);
        Task<IEnumerable<Entities.Attendance>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task AddAsync(Entities.Attendance attendance, CancellationToken cancellationToken = default);
        Task UpdateAsync(Entities.Attendance attendance, CancellationToken cancellationToken = default);
        Task<Entities.Attendance> GetByStudentAndDateAsync(Guid studentId, DateTime date, CancellationToken cancellationToken = default);
    }
}