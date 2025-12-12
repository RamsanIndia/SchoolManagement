using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student> GetByIdAsync(Guid id,CancellationToken cancellationToken);
        Task<Student> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken);
        Task<IEnumerable<Student>> GetByClassAsync(Guid classId, CancellationToken cancellationToken);
        Task<Student> CreateAsync(Student student, CancellationToken cancellationToken);
        Task<Student> UpdateAsync(Student student, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Student>> SearchAsync(string searchTerm, int page, int pageSize);

        Task<Student> GetByCodeAsync(string studentCode, CancellationToken cancellationToken = default);
    }
}
