using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Commands
{
    public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateTeacherCommandHandler> _logger;
        private readonly ITenantService _tenantService;

        public CreateTeacherCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CreateTeacherCommandHandler> logger,
            ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _tenantService = tenantService;
        }

        public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // ✅ Tenant/School from current user context
                var tenantId = _tenantService.TenantId;
                var schoolId = _tenantService.SchoolId;

                // ✅ Business validation: Unique EmployeeCode in tenant/school
                var existingTeacher = await _unitOfWork.TeachersRepository
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId
                                           && t.SchoolId == schoolId
                                           && t.EmployeeCode == request.EmployeeCode.ToUpperInvariant(),
                                      cancellationToken);

                if (existingTeacher != null)
                {
                    return Result<Guid>.Failure("EmployeeCodeExists",
                        $"Employee Code '{request.EmployeeCode}' already exists in this school");
                }

                // ✅ Unique Email in tenant/school
                var existingEmail = await _unitOfWork.TeachersRepository
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId
                                           && t.SchoolId == schoolId
                                           && t.Email.Value == request.Email.ToLowerInvariant(),
                                      cancellationToken);

                if (existingEmail != null)
                {
                    return Result<Guid>.Failure("EmailExists",
                        $"Email '{request.Email}' already registered in this school");
                }

                // ✅ Department exists
                if (request.DepartmentId.HasValue)
                {
                    var department = await _unitOfWork.DepartmentRepository
                        .GetByIdAsync(request.DepartmentId.Value, cancellationToken);
                    if (department == null)
                    {
                        return Result<Guid>.Failure("DepartmentNotFound",
                            $"Department '{request.DepartmentId}' not found");
                    }
                }

                // ✅ Value Objects
                var name = new FullName(request.FirstName.Trim(), request.LastName.Trim());
                var email = new Email(request.Email.Trim().ToLowerInvariant());
                var phone = new PhoneNumber(request.PhoneNumber?.Trim());
                var address = request.Address != null
                    ? new Address(request.Address.Street, request.Address.City,
                                  request.Address.State, request.Address.ZipCode, request.Address.Country)
                    : null;

                // ✅ Factory method - EXACT match
                var teacher = Teacher.Create(
                    tenantId: tenantId,
                    schoolId: (Guid)schoolId,
                    firstName: request.FirstName.Trim(),
                    lastName: request.LastName.Trim(),
                    email: request.Email.Trim().ToLowerInvariant(),
                    phoneNumber: request.PhoneNumber?.Trim(),
                    employeeCode: request.EmployeeCode.Trim().ToUpperInvariant(),
                    dateOfJoining: request.DateOfJoining.Date,
                    dateOfBirth: request.DateOfBirth.Date,
                    gender: request.Gender?.Trim(),
                    qualification: request.Qualification.Trim(),
                    priorExperience: request.PriorExperience,
                    address: address,
                    specialization: request.Specialization?.Trim(),
                    salary: request.Salary,
                    employmentType: request.EmploymentType?.Trim() ?? "Full-time",
                    departmentId: request.DepartmentId,
                    createdBy: _currentUserService.Username ?? "System",
                    createdIp: _currentUserService.IpAddress);

                // ✅ Persist
                await _unitOfWork.TeachersRepository.AddAsync(teacher, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Teacher created: ID={TeacherId}, Code={EmployeeCode}, School={SchoolId}",
                    teacher.Id, teacher.EmployeeCode, schoolId);

                return Result<Guid>.Success(teacher.Id,
                    $"Teacher '{teacher.Name.FullNameString}' created successfully");
            }
            catch (DomainException ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning(ex, "Domain validation failed: {Message}", ex.Message);
                return Result<Guid>.Failure("DomainValidationError", ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Failed to create teacher");
                return Result<Guid>.Failure("UnexpectedError", "Failed to create teacher");
            }
        }
    }
}
