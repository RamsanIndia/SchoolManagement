using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Repositories
{
    public class SectionSubjectRepository : Repository<SectionSubject>, ISectionSubjectRepository
    {
        public SectionSubjectRepository(SchoolManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SectionSubject>> GetBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(ss => ss.SectionId == sectionId && !ss.IsDeleted)
                .OrderBy(ss => ss.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<SectionSubject>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await _dbSet
                .Where(ss => ss.TeacherId == teacherId && !ss.IsDeleted)
                .Include(ss => ss.Section)
                    .ThenInclude(s => s.Class)
                .OrderBy(ss => ss.Section.Class.Grade)
                .ThenBy(ss => ss.SubjectName)
                .ToListAsync();
        }

        public async Task<bool> IsSubjectMappedAsync(Guid sectionId, Guid subjectId,CancellationToken cancellationToken)
        {
            return await _dbSet.AnyAsync(ss =>
                ss.SectionId == sectionId &&
                ss.SubjectId == subjectId &&
                !ss.IsDeleted);
        }

        public async Task<SectionSubject> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId)
        {
            return await _dbSet.FirstOrDefaultAsync(ss =>
                ss.SectionId == sectionId &&
                ss.SubjectId == subjectId &&
                !ss.IsDeleted);
        }
    }
}
