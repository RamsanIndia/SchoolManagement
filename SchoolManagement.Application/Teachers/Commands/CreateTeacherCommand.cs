using MediatR;
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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string Qualification { get; set; }
        public decimal Experience { get; set; }

        // Address details
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public Guid? DepartmentId { get; set; }
    }
}
