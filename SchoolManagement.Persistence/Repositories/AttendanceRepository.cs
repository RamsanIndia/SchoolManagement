using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly SchoolManagementDbContext _context;

        public AttendanceRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Attendance> CreateAsync(Attendance attendance,CancellationToken cancellationToken)
        {
            await _context.Attendances.AddAsync(attendance);
            return attendance;
        }

        public async Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(Guid studentId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == studentId &&
                           a.Date >= fromDate.Date &&
                           a.Date <= toDate.Date &&
                           !a.IsDeleted)
                .Include(a => a.Student)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetClassAttendanceAsync(Guid classId, DateTime date, CancellationToken cancellationToken)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.Student.ClassId == classId &&
                           a.Date.Date == date.Date &&
                           !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Attendance> GetTodayAttendanceAsync(Guid studentId, DateTime date, CancellationToken cancellationToken)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId &&
                                        a.Date.Date == date.Date &&
                                        !a.IsDeleted);
        }

        public async Task<AttendanceStatistics> GetAttendanceStatisticsAsync(Guid studentId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
        {
            var attendances = await GetStudentAttendanceAsync(studentId, fromDate, toDate,cancellationToken);
            var totalDays = attendances.Count();
            var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var lateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);

            return new AttendanceStatistics
            {
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = lateDays,
                AttendancePercentage = totalDays > 0 ? (decimal)presentDays / totalDays * 100 : 0
            };
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances
                .Where(a => !a.IsDeleted)
                .Include(a => a.Student)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.Date.Date == date.Date && !a.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
        {
            await _context.Attendances.AddAsync(attendance, cancellationToken);
        }

        public async Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
        {
            _context.Attendances.Update(attendance);
        }
        
        public async Task<Attendance> GetByStudentAndDateAsync(
            Guid studentId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == studentId &&
                    a.Date.Date == date.Date &&
                    !a.IsDeleted,
                    cancellationToken);
        }
    }
}
