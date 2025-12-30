using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Commands
{
    public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateTeacherCommandHandler> _logger;

        public CreateTeacherCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CreateTeacherCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            CreateTeacherCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Business validation: Check if employee ID already exists
                var existingTeacher = await _unitOfWork.TeachersRepository
                    .FirstOrDefaultAsync(t => t.EmployeeId == request.EmployeeId.ToUpper(), cancellationToken);

                if (existingTeacher != null)
                {
                    return Result<Guid>.Failure(
                        "EmployeeIdExists",
                        $"Employee ID '{request.EmployeeId}' is already in use"
                    );
                }

                // Business validation: Check if email already exists
                var existingEmail = await _unitOfWork.TeachersRepository
                    .FirstOrDefaultAsync(t => t.Email.Value == request.Email.ToLower(), cancellationToken);

                if (existingEmail != null)
                {
                    return Result<Guid>.Failure(
                        "EmailExists",
                        $"Email '{request.Email}' is already registered"
                    );
                }

                // Business validation: Verify department exists if provided
                if (request.DepartmentId.HasValue)
                {
                    var department = await _unitOfWork.DepartmentRepository
                        .GetByIdAsync(request.DepartmentId.Value, cancellationToken);

                    if (department == null)
                    {
                        return Result<Guid>.Failure(
                            "DepartmentNotFound",
                            $"Department with ID '{request.DepartmentId}' not found"
                        );
                    }
                }

                // Create address value object
                var address = new Address(
                    request.Street,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country
                );

                // Create teacher using factory method
                var teacher = Teacher.Create(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.EmployeeId,
                    request.DateOfJoining,
                    request.Qualification,
                    request.Experience,
                    address,
                    request.DepartmentId
                );

                //teacher.CreatedBy = _currentUserService.Username;

                // Persist to database
                await _unitOfWork.TeachersRepository.AddAsync(teacher, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Teacher created successfully. ID: {TeacherId}, EmployeeId: {EmployeeId}",
                    teacher.Id,
                    teacher.EmployeeId
                );

                return Result<Guid>.Success(
                    teacher.Id,
                    $"Teacher '{teacher.FullName}' created successfully"
                );
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation failed while creating teacher");
                return Result<Guid>.Failure("DomainValidationFailed", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher");
                return Result<Guid>.Failure(
                    "UnexpectedError",
                    "An unexpected error occurred while creating the teacher"
                );
            }
        }
    }
}
