using MediatR;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Students.Commands;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Students.Handler.Commands
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public CreateStudentCommandHandler(
            IStudentRepository studentRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _studentRepository = studentRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var address = new Address(
                    request.Address.Street,
                    request.Address.City,
                    request.Address.State,
                    request.Address.Country,
                    request.Address.ZipCode);

                var admissionNumber = string.IsNullOrEmpty(request.AdmissionNumber)
                    ? $"ADM{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : request.AdmissionNumber;

                var student = new Student(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.DateOfBirth,
                    (Gender)request.Gender,
                    request.ClassId,
                    request.SectionId,
                    admissionNumber,
                    request.AdmissionDate == default ? DateTime.UtcNow : request.AdmissionDate,
                    (StudentStatus)request.Status,
                    request.PhotoUrl
                );

                student.UpdatePersonalInfo(
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    middleName: request.MiddleName,
                    phone: request.Phone,
                    address: address
                );

                await _unitOfWork.StudentRepository.CreateAsync(student);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                if (!string.IsNullOrEmpty(request.Phone))
                {
                    await _notificationService.SendSMSAsync(
                        request.Phone,
                        $"Welcome to our school! Student ID: {student.StudentId}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                return Result.Failure("Error creating student:", ex.Message);
            }
        }
    }
}
