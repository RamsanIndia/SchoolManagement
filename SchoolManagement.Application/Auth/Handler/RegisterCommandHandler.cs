// Application/Auth/Handlers/RegisterCommandHandler.cs - ✅ UNIT OF WORK + request.SchoolId
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;  // ✅ SINGLE UoW for all repos
        private readonly IPasswordService _passwordService;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly ITenantService _tenantService;
        private readonly ICacheService _cacheService;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ILogger<RegisterCommandHandler> logger,
            ICurrentUserService currentUserService,
            ICorrelationIdService correlationIdService,
            ITenantService tenantService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdService.GetCorrelationId();

            try
            {
                _logger.LogInformation("🔑 Registration - Corr: {CorrelationId}, Email: {Email}, SchoolId: {SchoolId}",
                    correlationId, request.Email, request.SchoolId);

                // ✅ 1. SchoolId required
                if (request.SchoolId == Guid.Empty)
                    return Result<UserDto>.Failure("SchoolId required");

                // ✅ 2. Resolve context via UoW
                var (tenantId, schoolId) = await ResolveAdminSchoolContext(request.SchoolId, cancellationToken);
                _logger.LogDebug("Context: Tenant={TenantId}, School={SchoolId}", tenantId, schoolId);

                // ✅ 3. Uniqueness checks (tenant-scoped)
                var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, tenantId, cancellationToken);
                if (existingUser != null)
                    return Result<UserDto>.Failure("Email exists in organization");

                var existingUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username, tenantId, cancellationToken);
                if (existingUsername != null)
                    return Result<UserDto>.Failure("Username taken");

                // ✅ 4. Value objects
                var email = new Email(request.Email);
                var fullName = new FullName(request.FirstName, request.LastName);
                var phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber) ? new PhoneNumber(request.PhoneNumber) : null;

                var passwordHash = _passwordService.HashPassword(request.Password);
                var clientIp = _currentUserService.IpAddress ?? "Unknown";
                var createdBy = _currentUserService.Username ?? "System";

                // ✅ 5. Create with CORRECT SchoolId/TenantId
                var user = User.Create(tenantId, schoolId, request.Username, email, fullName,
                    passwordHash, request.UserType, createdBy, clientIp);

                if (phoneNumber != null)
                    user.UpdatePhoneNumber(phoneNumber, createdBy, clientIp);

                // ✅ 6. SINGLE SaveChanges via UoW
                await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                // ✅ Invalidate users list cache after creating new user
                await InvalidateUsersCacheAsync(cancellationToken);

                _logger.LogInformation("✅ Created UserId={UserId} SchoolId={SchoolId}", user.Id, schoolId);
                return Result<UserDto>.Success(MapUserToDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Registration failed - Corr: {CorrelationId}", correlationId);
                return Result<UserDto>.Failure("Registration failed");
            }
        }

        // ✅ FIXED: Uses request.SchoolId + UoW
        private async Task<(Guid tenantId, Guid schoolId)> ResolveAdminSchoolContext(Guid requestSchoolId, CancellationToken ct)
        {
            var serviceTenantId = _tenantService.TenantId;
            if (serviceTenantId == Guid.Empty)
                throw new UnauthorizedAccessException("Tenant required");

            var currentUserType = GetCurrentUserType();
            Guid schoolId;

            if (currentUserType == UserType.SuperAdmin)
            {
                // ✅ SUPERADMIN: request.SchoolId + validate exists
                var school = await _unitOfWork.SchoolRepository.GetByIdAsync(requestSchoolId, ct);
                if (school == null)
                    throw new InvalidOperationException($"School {requestSchoolId} not found");

                var tenantId = school.TenantId ?? Guid.Empty;  // 🎯 School's tenant!
                if (tenantId == Guid.Empty)
                    throw new InvalidOperationException($"School {requestSchoolId} has no tenant assigned");

                schoolId = requestSchoolId;  // 🎯 From request!
                _logger.LogDebug("SuperAdmin → School {SchoolId} (Tenant: {TenantId})", schoolId, tenantId);
                return (tenantId, schoolId);
            }
            else if (currentUserType == UserType.Admin)
            {
                // ✅ ADMIN: Own school only
                schoolId = _tenantService.SchoolId ?? Guid.Empty;
                if (schoolId == Guid.Empty)
                    throw new InvalidOperationException("Admin needs school assignment");

                _logger.LogDebug("Admin → Own school {SchoolId}", schoolId);
                return (serviceTenantId, schoolId);
            }
            else
            {
                throw new UnauthorizedAccessException("Only Admins/SuperAdmins register users");
            }
        }

        private async Task InvalidateUsersCacheAsync(CancellationToken ct)
        {
            var tenantId = _tenantService.TenantId;
            var schoolId = _tenantService.SchoolId ?? Guid.Empty;

            // ✅ Remove all cached user lists for this tenant/school
            var cachePattern = $"users:tenant:{tenantId}:school:{schoolId}:*";
            await _cacheService.RemoveByPatternAsync(cachePattern, ct);

            _logger.LogDebug("🗑️ Invalidated users cache: {Pattern}", cachePattern);
        }

        private UserType GetCurrentUserType()
        {
            return Enum.TryParse<UserType>(_currentUserService.UserType, out var type) ? type : UserType.SuperAdmin;
        }

        private static UserDto MapUserToDto(User user)  // ✅ NULL-SAFE
        {
            return new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email.Value,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = $"{user.FullName.FirstName} {user.FullName.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber?.Value,
                IsEmailVerified = user.EmailVerified,
                IsPhoneVerified = user.PhoneVerified,
                SchoolId = user.SchoolId?.ToString() ?? "",
                TenantId = user.TenantId.ToString(),
                Roles = new List<string> { user.UserType.ToString() },
                LastLoginAt = user.LastLoginAt ?? DateTime.MinValue,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive.ToString()
            };
        }

        private Result ValidateRegistrationRequest(RegisterCommand request)
        {
            // Your existing validation logic here (unchanged)
            return Result.Success();
        }
    }
}