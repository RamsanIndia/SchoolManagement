using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Auth.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUsersQueryHandler> _logger;

        public GetUsersQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetUsersQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Fetching users with pagination. PageNumber: {PageNumber}, PageSize: {PageSize}",
                    request.PageNumber, request.PageSize);

                // Build base query
                IQueryable<User> query = _unitOfWork.UserRepository.GetQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(u =>
                        (u.Email != null && u.Email.Value.ToLower().Contains(searchTerm)) ||
                        (u.FullName != null && u.FullName.FirstName.ToLower().Contains(searchTerm)) ||
                        (u.FullName != null && u.FullName.LastName.ToLower().Contains(searchTerm)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Value.Contains(searchTerm)));
                }

                // Apply user type filter
                if (!string.IsNullOrWhiteSpace(request.UserType))
                {
                    query = query.Where(u => u.UserType.ToString() == request.UserType);
                }

                // Apply email verified filter
                if (request.IsEmailVerified.HasValue)
                {
                    query = query.Where(u => u.EmailVerified == request.IsEmailVerified.Value);
                }

                // Apply phone verified filter
                if (request.IsPhoneVerified.HasValue)
                {
                    query = query.Where(u => u.PhoneVerified == request.IsPhoneVerified.Value);
                }

                // Apply active filter (if your User entity has IsActive property)
                // if (request.IsActive.HasValue)
                // {
                //     query = query.Where(u => u.IsActive == request.IsActive.Value);
                // }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "email" => request.SortDirection == "desc"
                        ? query.OrderByDescending(u => u.Email.Value)
                        : query.OrderBy(u => u.Email.Value),
                    "lastname" => request.SortDirection == "desc"
                        ? query.OrderByDescending(u => u.FullName.LastName)
                        : query.OrderBy(u => u.FullName.LastName),
                    "usertype" => request.SortDirection == "desc"
                        ? query.OrderByDescending(u => u.UserType)
                        : query.OrderBy(u => u.UserType),
                    "createdat" => request.SortDirection == "desc"
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.FullName.FirstName) // Default: sort by first name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and include related data
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .ToListAsync(cancellationToken);

                // Map to DTOs with null-safe value object access
                var dtos = items.Select(user => new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email?.Value ?? string.Empty,
                    FirstName = user.FullName?.FirstName ?? string.Empty,
                    LastName = user.FullName?.LastName ?? string.Empty,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber?.Value,
                    LastLoginAt = user.LastLoginAt ?? DateTime.MinValue,
                    IsEmailVerified = user.EmailVerified,
                    IsPhoneVerified = user.PhoneVerified,
                    Roles = user.UserRoles?.Select(ur => ur.Role?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList()
                        ?? new List<string> { user.UserType.ToString() }
                }).ToList();

                // Create paged result
                var response = new PagedResult<UserDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                _logger.LogDebug("Successfully fetched {Count} users out of {Total}",
                    dtos.Count, totalCount);

                return Result<PagedResult<UserDto>>.Success(
                    response,
                    "Users fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return Result<PagedResult<UserDto>>.Failure(
                    "Failed to fetch users.",
                    ex.Message);
            }
        }
    }
}
