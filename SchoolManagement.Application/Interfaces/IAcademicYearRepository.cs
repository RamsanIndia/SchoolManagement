using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{

    public interface IAcademicYearRepository : IRepository<AcademicYear>
    {
        Task<AcademicYear?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<AcademicYear?> GetCurrentAcademicYearAsync(CancellationToken cancellationToken = default);
        Task<List<AcademicYear>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<AcademicYear?> GetByIdWithClassesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<AcademicYear>> GetCurrentYearsAsync(CancellationToken cancellationToken = default);
    }

}
