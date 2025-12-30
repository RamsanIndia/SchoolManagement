using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IClassRepository : IRepository<Class>
    {
        Task<Class> GetByIdWithSectionsAsync(Guid id,CancellationToken cancellationToken);
        Task<IEnumerable<Class>> GetByAcademicYearAsync(Guid academicYearId);
        Task<IEnumerable<Class>> GetActiveClassesAsync();
        Task<bool> IsClassCodeExistsAsync(string classCode,CancellationToken cancellationToken, Guid? excludeId = null);
        Task<(IEnumerable<Class> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string searchTerm, bool? isActive);

        Task<bool> IsClassNameExistsAsync(string className,  CancellationToken cancellationToken = default);
    }
}
