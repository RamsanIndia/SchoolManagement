using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Classes.Handlers.Commands
{
    public class DeactivateClassCommandHandler : IRequestHandler<DeactivateClassCommand, Result>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateClassCommandHandler> _logger;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly ICurrentUserService _currentUserService;

        public DeactivateClassCommandHandler(
            IClassRepository classRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateClassCommandHandler> logger,
            IpAddressHelper ipAddressHelper,
            ICorrelationIdService correlationIdService,
            ICurrentUserService currentUserService)
        {
            _classRepository = classRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ipAddressHelper = ipAddressHelper;
            _correlationIdService = correlationIdService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(
            DeactivateClassCommand request,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            var userId = _currentUserService.Username;

            try
            {
                _logger.LogInformation(
                    "Starting class deactivation. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, request.ClassId);

                var classEntity = await _classRepository.GetByIdAsync(
                    request.ClassId,
                    cancellationToken);

                if (classEntity == null)
                {
                    _logger.LogWarning(
                        "Class not found. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                        correlationId, request.ClassId);
                    return Result.Failure("Class not found.");
                }

                // ✅ Pass correlationId
                classEntity.Deactivate(userId);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Class deactivated successfully. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, classEntity.Id);

                return Result.Success("Class deactivated successfully.");
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex,
                    "Domain validation failed during class deactivation. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, request.ClassId);
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during class deactivation. CorrelationId: {CorrelationId}, ClassId: {ClassId}",
                    correlationId, request.ClassId);
                return Result.Failure("Failed to deactivate class. Please try again later.");
            }
        }
    }
}
