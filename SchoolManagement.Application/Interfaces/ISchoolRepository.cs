using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    // Application/Interfaces/ISchoolRepository.cs
    public interface ISchoolRepository : IRepository<School>
    {
        Task<School?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<School?> GetByCodeWithUsersAsync(string code, CancellationToken cancellationToken = default);
        Task<List<School>> GetActiveSchoolsAsync(CancellationToken cancellationToken = default, SchoolType? type = null);
        Task<List<School>> GetSchoolsByLocationAsync(string city, string state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets school with tenant information for token generation
        /// </summary>
        Task<SchoolWithTenantDto> GetSchoolWithTenantAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default);
    }

}
