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
        Task<bool> IsSectionNameExistsAsync(Guid classId, string sectionName,
            CancellationToken cancellationToken = default);

        Task<bool> IsSectionNameExistsExceptAsync(Guid classId, string sectionName,
            Guid exceptSectionId, CancellationToken cancellationToken = default);

        Task<Section?> GetByIdWithDetailsAsync(Guid id,
            CancellationToken cancellationToken = default);

        Task<int> GetActiveSectionCountByClassAsync(Guid classId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Section>> GetSectionsByClassIdAsync(Guid classId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a section by the assigned class teacher ID
        /// </summary>
        Task<Section?> GetSectionByClassTeacherIdAsync(
            Guid teacherId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all sections for a specific class with their class teachers
        /// </summary>
        Task<IEnumerable<Section>> GetSectionsByClassIdWithTeachersAsync(
            Guid classId,
            CancellationToken cancellationToken = default);
    }
}
