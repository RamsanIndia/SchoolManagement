// Application/Auth/Handler/GetUserProfileQueryHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result<UserDto>> Handle(
            GetUserProfileQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Fetching user profile for UserId: {UserId}", request.UserId);

                // Fetch user from repository
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<UserDto>.Failure("User not found");
                }

                // Check if user is deleted
                if (user.IsDeleted)
                {
                    _logger.LogWarning("Attempt to access deleted user profile: {UserId}", request.UserId);
                    return Result<UserDto>.Failure("User not found");
                }

                // Map to DTO
                var userDto = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email.Value,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    PhoneNumber = user.PhoneNumber?.Value,
                    IsEmailVerified = user.EmailVerified,
                    IsPhoneVerified = user.PhoneVerified,
                    //IsActive = user.IsActive,
                    //LastLoginAt = user.LastLoginAt,
                    Roles = new List<string> { user.UserType.ToString() }
                };

                _logger.LogDebug("Successfully fetched user profile for UserId: {UserId}", request.UserId);

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for UserId: {UserId}", request.UserId);
                return Result<UserDto>.Failure("An error occurred while retrieving user profile");
            }
        }
    }
}
