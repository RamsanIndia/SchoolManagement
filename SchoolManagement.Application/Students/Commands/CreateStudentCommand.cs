using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;

namespace SchoolManagement.Application.Students.Commands
{
    public class CreateStudentCommand : IRequest<Result<StudentDto>>
    {
        // ✅ REQUIRED: Multi-tenant context (from claims/header/context)
        public Guid TenantId { get; set; }
        public Guid SchoolId { get; set; }

        // ✅ Core personal info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Gender { get; set; } // Matches enum cast

        // ✅ Academic assignment
        public Guid ClassId { get; set; }
        public Guid SectionId { get; set; }

        // ✅ Admission details
        public string? AdmissionNumber { get; set; }
        public DateTime? AdmissionDate { get; set; } = DateTime.UtcNow;

        // ✅ Status & media
        public int Status { get; set; } = (int)StudentStatus.Active;
        public string? PhotoUrl { get; set; }

        // ✅ Address DTO matching ValueObject constructor
        public AddressDto? Address { get; set; }
    }

    // ✅ Matching AddressDto (align with Domain.ValueObjects.Address)
    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }  // Was ZipCode
        public string? Country { get; set; }
    }
}
