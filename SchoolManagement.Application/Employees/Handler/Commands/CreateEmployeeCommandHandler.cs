using MediatR;
using SchoolManagement.Application.Employees.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Employees.Handler.Commands
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmployeeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Create Address Value Object
                var address = new Address(
                    request.Address.Street,
                    request.Address.City,
                    request.Address.State,
                    request.Address.Country,
                    request.Address.ZipCode
                );

                // Convert int → Enum
                var genderEnum = (Gender)request.Gender;
                var employmentTypeEnum = (EmploymentType)request.EmploymentType;

                // Create main Employee entity
                var employee = new Employee(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.DateOfBirth,
                    genderEnum,
                    request.DepartmentId,
                    request.DesignationId,
                    employmentTypeEnum
                );

                // Update personal info (MiddleName, Phone, Address)
                employee.UpdatePersonalInfo(
                    request.FirstName,
                    request.LastName,
                    request.MiddleName,
                    request.Phone,
                    address
                );

                await _unitOfWork.EmployeeRepository.CreateAsync(employee, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Employee created successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Error creating employee.", new[] { ex.Message });
            }
        }
    }
}
