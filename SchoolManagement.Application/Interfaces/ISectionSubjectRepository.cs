using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ISectionSubjectRepository : IRepository<SectionSubject>
    {
        Task<IEnumerable<SectionSubject>> GetBySectionIdAsync(Guid sectionId,CancellationToken cancellationToken);
        Task<IEnumerable<SectionSubject>> GetByTeacherIdAsync(Guid teacherId);
        Task<bool> IsSubjectMappedAsync(Guid sectionId, Guid subjectId,CancellationToken cancellationToken);
        Task<SectionSubject> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId);
    }
}
