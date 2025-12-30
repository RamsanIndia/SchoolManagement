using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;

namespace SchoolManagement.Application.Classes.Commands
{
    /// <summary>
    /// Command to create a new class in the system
    /// </summary>
    public class CreateClassCommand : IRequest<Result<ClassDto>>
    {
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public int Grade { get; set; }
        public string Description { get; set; }
        public Guid AcademicYearId { get; set; }

    }
}
