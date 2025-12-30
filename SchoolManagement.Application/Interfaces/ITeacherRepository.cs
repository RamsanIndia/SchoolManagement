using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ITeacherRepository : IRepository<Teacher>
    {
        /// <summary>
        /// Gets teacher with department details
        /// </summary>
        Task<Teacher?> GetTeacherWithDetailsAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets teacher with teaching assignments (sections and subjects)
        /// </summary>
        Task<Teacher?> GetTeacherWithTeachingAssignmentsAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active teachers
        /// </summary>
        Task<IEnumerable<Teacher>> GetActiveTeachersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets teacher by email address
        /// </summary>
        Task<Teacher?> GetTeacherByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if teacher is active
        /// </summary>
        Task<bool> IsTeacherActiveAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets teachers by department
        /// </summary>
        Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(
            Guid departmentId,
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets teachers who can accept more teaching assignments
        /// </summary>
        Task<IEnumerable<Teacher>> GetAvailableTeachersAsync(
            int maxWeeklyPeriods = 40,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets teachers teaching a specific subject
        /// </summary>
        Task<IEnumerable<Teacher>> GetTeachersBySubjectAsync(
            Guid subjectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all class teachers
        /// </summary>
        Task<IEnumerable<Teacher>> GetClassTeachersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if employee ID is unique
        /// </summary>
        Task<bool> IsEmployeeIdUniqueAsync(
            string employeeId,
            Guid? excludeTeacherId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if email is unique
        /// </summary>
        Task<bool> IsEmailUniqueAsync(
            string email,
            Guid? excludeTeacherId = null,
            CancellationToken cancellationToken = default);
    }
}
