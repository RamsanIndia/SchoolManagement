using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        private readonly SchoolManagementDbContext _context;

        public TeacherRepository(SchoolManagementDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Teacher?> GetTeacherWithDetailsAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.TeachingAssignments) // Changed from TeacherSubjects
                    .ThenInclude(ta => ta.Section)
                .Include(t => t.ClassTeacherSections) // Include class teacher assignments
                    .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);
        }

        public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name.FirstName) // Use Name.FirstName instead of FirstName
                .ThenBy(t => t.Name.LastName)   // Use Name.LastName instead of LastName
                .ToListAsync(cancellationToken);
        }

        public async Task<Teacher?> GetTeacherByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            // Normalize email to lowercase for comparison
            var emailLower = email.ToLower();

            return await _context.Teachers
                .FirstOrDefaultAsync(
                    t => t.Email.Value == emailLower, // Use Email.Value (it's already lowercase)
                    cancellationToken
                );
        }

        public async Task<bool> IsTeacherActiveAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .AnyAsync(
                    t => t.Id == teacherId && t.IsActive,
                    cancellationToken
                );
        }

        /// <summary>
        /// Gets teacher with their teaching assignments (sections and subjects)
        /// </summary>
        public async Task<Teacher?> GetTeacherWithTeachingAssignmentsAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.TeachingAssignments)
                    .ThenInclude(ta => ta.Section)
                        .ThenInclude(s => s.Class)
                .Include(t => t.ClassTeacherSections)
                    .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);
        }

        /// <summary>
        /// Gets all teachers by department with their details
        /// </summary>
        public async Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(
            Guid departmentId,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Teachers
                .Include(t => t.Department)
                .Where(t => t.DepartmentId == departmentId);

            if (activeOnly)
            {
                query = query.Where(t => t.IsActive);
            }

            return await query
                .OrderBy(t => t.Name.FirstName)
                .ThenBy(t => t.Name.LastName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets teachers who can accept more teaching assignments
        /// </summary>
        public async Task<IEnumerable<Teacher>> GetAvailableTeachersAsync(
            int maxWeeklyPeriods = 40,
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Include(t => t.TeachingAssignments)
                .Include(t => t.Department)
                .Where(t => t.IsActive)
                .ToListAsync(cancellationToken)
                .ContinueWith(task =>
                {
                    // Filter in memory after loading (because CanAcceptMoreAssignments is a method)
                    return task.Result
                        .Where(t => t.CanAcceptMoreAssignments(maxWeeklyPeriods))
                        .OrderBy(t => t.GetTotalWeeklyPeriods())
                        .AsEnumerable();
                }, cancellationToken);
        }

        /// <summary>
        /// Gets teachers teaching a specific subject
        /// </summary>
        public async Task<IEnumerable<Teacher>> GetTeachersBySubjectAsync(
            Guid subjectId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.TeachingAssignments)
                .Where(t => t.IsActive &&
                       t.TeachingAssignments.Any(ta => ta.SubjectId == subjectId))
                .OrderBy(t => t.Name.FirstName)
                .ThenBy(t => t.Name.LastName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets teachers assigned as class teachers
        /// </summary>
        public async Task<IEnumerable<Teacher>> GetClassTeachersAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.ClassTeacherSections)
                    .ThenInclude(s => s.Class)
                .Where(t => t.IsActive && t.ClassTeacherSections.Any(s => s.IsActive))
                .OrderBy(t => t.Name.FirstName)
                .ThenBy(t => t.Name.LastName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Checks if employee ID is unique
        /// </summary>
        public async Task<bool> IsEmployeeIdUniqueAsync(
            string employeeId,
            Guid? excludeTeacherId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Teachers
                .Where(t => t.EmployeeCode == employeeId.ToUpper());

            if (excludeTeacherId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTeacherId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        /// <summary>
        /// Checks if email is unique
        /// </summary>
        public async Task<bool> IsEmailUniqueAsync(
            string email,
            Guid? excludeTeacherId = null,
            CancellationToken cancellationToken = default)
        {
            var emailLower = email.ToLower();
            var query = _context.Teachers
                .Where(t => t.Email.Value == emailLower);

            if (excludeTeacherId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTeacherId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }
    }
}
