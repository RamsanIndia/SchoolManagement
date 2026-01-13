using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Students.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;

namespace SchoolManagement.Application.Students.Handler.Commands
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService; // Add for audit

        public CreateStudentCommandHandler(
            IStudentRepository studentRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _studentRepository = studentRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<StudentDto>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // ✅ Build Address Value Object
                var address = request.Address != null
                    ? new Address(request.Address.Street, request.Address.City,
                                  request.Address.State, request.Address.PostalCode, request.Address.Country)
                    : null;

                // ✅ Use exact Student.Create factory method
                var student = Student.Create(
                    tenantId: request.TenantId,           // From command/command context
                    schoolId: request.SchoolId,           // From command/command context
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    email: request.Email,
                    dateOfBirth: request.DateOfBirth,
                    gender: (Gender)request.Gender,
                    classId: request.ClassId,
                    sectionId: request.SectionId,
                    middleName: request.MiddleName,
                    phone: request.Phone,
                    address: address,
                    admissionNumber: request.AdmissionNumber,
                    admissionDate: request.AdmissionDate ?? DateTime.UtcNow,
                    createdBy: _currentUserService.Username ?? "System",
                    createdIp: _currentUserService.IpAddress);

                // ✅ Create via repository (sets audit if needed)
                await _studentRepository.CreateAsync(student, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // ✅ Notification
                if (!string.IsNullOrEmpty(student.Phone))
                {
                    await _notificationService.SendSMSAsync(
                        student.Phone,
                        $"Welcome! Student ID: {student.Id}, Code: {student.StudentCode}");
                }

                return Result<StudentDto>.Success(new StudentDto
                {
                    Id = student.Id,
                    StudentId = student.StudentCode,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MiddleName = student.MiddleName,
                    Email = student.Email,
                    Phone = student.Phone,
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender.ToString(),
                    Status = student.Status.ToString(),
                    Address = student.Address != null ? new SchoolManagement.Application.DTOs.AddressDto
                    {
                        Street = student.Address.Street,
                        City = student.Address.City,
                        State = student.Address.State,
                        ZipCode = student.Address.PostalCode,
                        Country = student.Address.Country
                    } : null,
                    AdmissionDate = student.AdmissionDate,
                    PhotoUrl = student.PhotoUrl,
                    BiometricEnrolled = student.BiometricInfo != null,
                    // Class and Section mapping can be added if available
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<StudentDto>.Failure($"Error creating student: {ex.Message}");
            }
        }
    }
}
