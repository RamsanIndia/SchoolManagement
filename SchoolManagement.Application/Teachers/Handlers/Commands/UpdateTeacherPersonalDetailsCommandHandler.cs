using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Commands
{
    public class UpdateTeacherPersonalDetailsCommandHandler
        : IRequestHandler<UpdateTeacherPersonalDetailsCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateTeacherPersonalDetailsCommandHandler> _logger;

        public UpdateTeacherPersonalDetailsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateTeacherPersonalDetailsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            UpdateTeacherPersonalDetailsCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var teacher = await _unitOfWork.TeachersRepository
                    .GetByIdAsync(request.TeacherId, cancellationToken);

                if (teacher == null)
                {
                    return Result<bool>.Failure(
                        "TeacherNotFound",
                        $"Teacher with ID '{request.TeacherId}' not found"
                    );
                }

                var address = new Address(
                    request.Street,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country
                );

                teacher.UpdatePersonalDetails(
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    address,
                    _currentUserService.Username
                );

                await _unitOfWork.TeachersRepository.UpdateAsync(teacher, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Teacher personal details updated. TeacherId: {TeacherId}",
                    teacher.Id
                );

                return Result<bool>.Success(
                    true,
                    $"Personal details updated for '{teacher.FullName}'"
                );
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation failed");
                return Result<bool>.Failure("DomainValidationFailed", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher personal details");
                return Result<bool>.Failure("UnexpectedError", "An error occurred");
            }
        }
    }
}
