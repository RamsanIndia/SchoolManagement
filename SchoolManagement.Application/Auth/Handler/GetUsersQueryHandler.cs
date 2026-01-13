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
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<GetUsersQueryHandler> _logger;

        public GetUsersQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ITenantService tenantService,
            ILogger<GetUsersQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Generate cache key based on query parameters and tenant context
                var cacheKey = GenerateCacheKey(request);

                _logger.LogDebug(
                    "Fetching users - PageNumber: {PageNumber}, PageSize: {PageSize}, CacheKey: {CacheKey}",
                    request.PageNumber, request.PageSize, cacheKey);

                // ✅ Use cache with 5-minute expiration and 2-minute sliding window
                var result = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () => await FetchUsersFromDatabase(request, cancellationToken),
                    absoluteExpiration: TimeSpan.FromMinutes(5),
                    slidingExpiration: TimeSpan.FromMinutes(2),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation(
                    "✅ Successfully fetched {Count} users out of {Total} (Page {Page}/{TotalPages})",
                    result.Items.Count(),
                    result.TotalCount,
                    request.PageNumber,
                    result.TotalPages);

                return Result<PagedResult<UserDto>>.Success(
                    result,
                    $"Successfully fetched {result.Items.Count()} users.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error fetching users");
                return Result<PagedResult<UserDto>>.Failure(
                    "Failed to fetch users.",
                    ex.Message);
            }
        }

        /// <summary>
        /// Generate unique cache key based on query parameters and tenant context
        /// </summary>
        private string GenerateCacheKey(GetUsersQuery request)
        {
            var tenantId = _tenantService.TenantId;
            var schoolId = _tenantService.SchoolId ?? Guid.Empty;

            // ✅ Include all filter parameters in cache key for proper isolation
            return $"users:" +
                   $"tenant:{tenantId}:" +
                   $"school:{schoolId}:" +
                   $"page:{request.PageNumber}:" +
                   $"size:{request.PageSize}:" +
                   $"search:{request.SearchTerm?.ToLower() ?? "none"}:" +
                   $"type:{request.UserType?.ToLower() ?? "all"}:" +
                   $"emailVerified:{request.IsEmailVerified?.ToString() ?? "all"}:" +
                   $"phoneVerified:{request.IsPhoneVerified?.ToString() ?? "all"}:" +
                   $"sort:{request.SortBy?.ToLower() ?? "default"}:" +
                   $"dir:{request.SortDirection?.ToLower() ?? "asc"}";
        }

        /// <summary>
        /// Fetch users from database (called on cache miss)
        /// </summary>
        private async Task<PagedResult<UserDto>> FetchUsersFromDatabase(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("📊 Fetching users from database (cache miss)");

            // Build base query with includes FIRST
            IQueryable<User> query = _unitOfWork.UserRepository.GetQueryable()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role);

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
                "lastlogin" => request.SortDirection == "desc"
                    ? query.OrderByDescending(u => u.LastLoginAt)
                    : query.OrderBy(u => u.LastLoginAt),
                _ => query.OrderBy(u => u.FullName.FirstName)
            };

            // Get total count BEFORE pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var dtos = items.Select(user => new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email?.Value ?? string.Empty,
                FirstName = user.FullName?.FirstName ?? string.Empty,
                LastName = user.FullName?.LastName ?? string.Empty,
                Username = user.Username ?? string.Empty,
                PhoneNumber = user.PhoneNumber?.Value,
                LastLoginAt = user.LastLoginAt ?? DateTime.MinValue,
                IsEmailVerified = user.EmailVerified,
                IsPhoneVerified = user.PhoneVerified,
                UserType = user.UserType.ToString(),
                Roles = (user.UserRoles != null && user.UserRoles.Any())
                    ? user.UserRoles
                        .Where(ur => ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name) && ur.IsActive && !ur.IsDeleted)
                        .Select(ur => ur.Role.Name)
                        .Distinct()
                        .ToList()
                    : new List<string> { user.UserType.ToString() }
            }).ToList();

            // Return paged result
            return new PagedResult<UserDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
