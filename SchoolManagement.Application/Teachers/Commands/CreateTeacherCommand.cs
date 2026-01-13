using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Commands
{
    /// <summary>
    /// Command to create a new teacher
    /// </summary>
    public class CreateTeacherCommand : IRequest<Result<Guid>>
    {
        public Guid TenantId { get; set; }
        public Guid SchoolId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public decimal PriorExperience { get; set; }
        public AddressDto? Address { get; set; }
        public string? Specialization { get; set; }
        public decimal Salary { get; set; }
        public string? EmploymentType { get; set; } = "Full-time";
        public Guid? DepartmentId { get; set; }
    }

}
