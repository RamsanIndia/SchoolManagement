using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        /// <summary>
        /// Gets department with all teachers
        /// </summary>
        Task<Department?> GetDepartmentWithTeachersAsync(
            Guid departmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active departments
        /// </summary>
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if department name is unique
        /// </summary>
        Task<bool> IsDepartmentNameUniqueAsync(
            string name,
            Guid? excludeDepartmentId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if department code is unique
        /// </summary>
        Task<bool> IsDepartmentCodeUniqueAsync(
            string code,
            Guid? excludeDepartmentId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets department by code
        /// </summary>
        Task<Department?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);
    }
}
