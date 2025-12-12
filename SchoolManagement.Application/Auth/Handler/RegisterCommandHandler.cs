// Application/Auth/Handlers/RegisterCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IpAddressHelper _ipAddressHelper;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IpAddressHelper ipAddressHelper,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ILogger<RegisterCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _ipAddressHelper=ipAddressHelper;
        }

        public async Task<Result<UserDto>> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var correlationId = _ipAddressHelper.GetCorrelationId();

            try
            {
                _logger.LogInformation("Starting user registration. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId, request.Email);

                // Double-check uniqueness (defense in depth - FluentValidation may be bypassed)
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                {
                    _logger.LogWarning("Duplicate registration attempt. CorrelationId: {CorrelationId}, Email: {Email}",
                        correlationId, request.Email);
                    return Result<UserDto>.Failure("User with this email already exists.");
                }

                // Create domain value objects with validation
                var email = new Email(request.Email);
                var fullName = new FullName(request.FirstName, request.LastName);

                PhoneNumber phoneNumber = null;
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    phoneNumber = new PhoneNumber(request.PhoneNumber);
                }

                // Hash password using enterprise-grade service
                var passwordHash = _passwordService.HashPassword(request.Password);

                // Get client IP for audit trail
                var clientIp = _ipAddressHelper.GetClientIpAddress(); 

                // Create user aggregate using factory method (raises UserCreatedEvent)
                var user = User.Create(
                    username: request.Username,
                    email: email,
                    fullName: fullName,
                    passwordHash: passwordHash,
                    userType: request.UserType,
                    createdBy: "System",
                    createdIp: clientIp,
                    correlationId: correlationId);

                // Update phone number if provided (raises domain event)
                if (phoneNumber != null)
                {
                    user.UpdatePhoneNumber(phoneNumber, correlationId);
                }

                // Add to repository (aggregate root manages consistency)
                await _userRepository.AddAsync(user, cancellationToken);

                // Persist changes and dispatch domain events (UserCreatedEvent, etc.)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User registered successfully. CorrelationId: {CorrelationId}, UserId: {UserId}, Email: {Email}, UserType: {UserType}",
                    correlationId, user.Id, user.Email.Value, user.UserType);

                // Map to DTO using AutoMapper or manual mapping (no direct entity exposure)
                var userDto = new UserDto
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Email = user.Email.Value,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    PhoneNumber = user.PhoneNumber?.Value,
                    IsEmailVerified = user.EmailVerified,
                    IsPhoneVerified = user.PhoneVerified,
                    //UserType = user.UserType.ToString(),
                    Roles = new List<string> { user.UserType.ToString() },
                    //LastLoginAt=user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Result<UserDto>.Success(userDto, "User registered successfully");
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain validation failed during registration. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId, request.Email);
                return Result<UserDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId, request.Email);
                return Result<UserDto>.Failure("Registration failed. Please try again later.");
            }
        }

    }
}
