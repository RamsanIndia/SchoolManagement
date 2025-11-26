using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.UserRoles.Commands;
using SchoolManagement.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.UserRoles.Handler.Commands
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssignRoleToUserCommandHandler> _logger;
        private const int MaxRetryCount = 3;

        public AssignRoleToUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ILogger<AssignRoleToUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty || request.RoleId == Guid.Empty)
                return Result.Failure("Invalid UserId or RoleId");

            int retryCount = 0;

            while (retryCount < MaxRetryCount)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Load the user
                    var user = await _userRepository.GetByIdAsync(request.UserId);
                    if (user == null)
                        return Result.Failure("User not found");

                    if (!user.IsActive)
                        return Result.Failure("User account is deactivated");

                    // Load the role
                    var role = await _roleRepository.GetByIdAsync(request.RoleId);
                    if (role == null)
                        return Result.Failure("Role not found");

                    // Check if the user already has this role
                    var existingUserRole = await _unitOfWork.UserRoleRepository
                        .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId && ur.IsActive, cancellationToken);

                    if (existingUserRole != null)
                        return Result.Failure("User already has this active role");

                    // Create new UserRole
                    var userRole = new UserRole(user.Id, role.Id, DateTime.UtcNow, true, request.ExpiresAt);

                    await _unitOfWork.UserRoleRepository.AddAsync(userRole, cancellationToken);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation("Role {RoleId} assigned to User {UserId} successfully", role.Id, user.Id);

                    return Result.Success("Role assigned successfully");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Concurrency conflict assigning role {RoleId} to user {UserId}. Retry {Retry}/{MaxRetry}", request.RoleId, request.UserId, retryCount, MaxRetryCount);

                    if (retryCount >= MaxRetryCount)
                        return Result.Failure("Concurrency conflict, please retry.");

                    // Reload the tracked entities to get latest values
                    foreach (var entry in ex.Entries)
                        await entry.ReloadAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
                    return Result.Failure("An unexpected error occurred while assigning role");
                }
            }

            return Result.Failure("Failed to assign role due to repeated concurrency conflicts");
        }
    }
}
