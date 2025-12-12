using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ISectionRepository : IRepository<Section>
    {
        Task<Section> GetByIdWithDetailsAsync(Guid id,CancellationToken cancellationToken);
        Task<IEnumerable<Section>> GetByClassIdAsync(Guid classId,CancellationToken cancellationToken);
        Task<IEnumerable<Section>> GetActiveSectionsAsync(CancellationToken cancellationToken);
        Task<bool> IsSectionNameExistsAsync(Guid classId, string sectionName, Guid? excludeId = null);
        Task<IEnumerable<Section>> GetSectionsByTeacherAsync(Guid teacherId);
        Task<int> GetStudentCountAsync(Guid sectionId);
    }
}
