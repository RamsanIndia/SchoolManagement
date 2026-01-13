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
    public class StudentRepository : IStudentRepository
    {
        private readonly SchoolManagementDbContext _context;

        public StudentRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Student> GetByIdAsync(Guid id,CancellationToken cancellationToken)
        {
            return await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<Student> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentCode == studentId && !s.IsDeleted);
        }

        public async Task<IEnumerable<Student>> GetByClassAsync(Guid classId, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Where(s => s.ClassId == classId && !s.IsDeleted)
                .Include(s => s.Class)
                .Include(s => s.Section)
                .ToListAsync();
        }

        public async Task<Student> CreateAsync(Student student, CancellationToken cancellationToken)
        {
            _context.Students.Add(student);
            return student;
        }

        public async Task<Student> UpdateAsync(Student student, CancellationToken cancellationToken)
        {
            _context.Students.Update(student);
            return student;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var student = await GetByIdAsync(id, cancellationToken);
            if (student != null)
            {
                student.MarkAsDeleted();
                _context.Students.Update(student);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Students.AnyAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<IEnumerable<Student>> SearchAsync(string searchTerm, int page, int pageSize)
        {
            return await _context.Students
                .Where(s => !s.IsDeleted &&
                           (s.FirstName.Contains(searchTerm) ||
                            s.LastName.Contains(searchTerm) ||
                            s.StudentCode.Contains(searchTerm) ||
                            s.Email.Contains(searchTerm)))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Student> GetByCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.AdmissionNumber == studentCode && !s.IsDeleted, cancellationToken);
        }
    }
}
