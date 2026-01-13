using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Roles.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Roles.Handler.Queries
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<PagedResult<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;

        public GetAllRolesQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ITenantService tenantService,
            ILogger<GetAllRolesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<Result<PagedResult<RoleDto>>> Handle(
            GetAllRolesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Generate cache key based on query parameters and tenant context
                var cacheKey = GenerateCacheKey(request);

                _logger.LogDebug(
                    "Fetching roles - PageNumber: {PageNumber}, PageSize: {PageSize}, CacheKey: {CacheKey}",
                    request.PageNumber, request.PageSize, cacheKey);

                // ✅ Use cache with 30-minute expiration (roles change less frequently)
                var result = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () => await FetchRolesFromDatabase(request, cancellationToken),
                    absoluteExpiration: TimeSpan.FromMinutes(30),
                    slidingExpiration: TimeSpan.FromMinutes(10),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation(
                    "✅ Successfully fetched {Count} roles out of {Total} (Page {Page}/{TotalPages})",
                    result.Items.Count(),
                    result.TotalCount,
                    request.PageNumber,
                    result.TotalPages);

                return Result<PagedResult<RoleDto>>.Success(
                    result,
                    "Roles fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error fetching roles");
                return Result<PagedResult<RoleDto>>.Failure(
                    "Failed to fetch roles.",
                    ex.Message);
            }
        }

        /// <summary>
        /// Generate unique cache key based on query parameters and tenant context
        /// </summary>
        private string GenerateCacheKey(GetAllRolesQuery request)
        {
            var tenantId = _tenantService.TenantId;
            var schoolId = _tenantService.SchoolId ?? Guid.Empty;

            // ✅ Include all filter parameters in cache key
            return $"roles:" +
                   $"tenant:{tenantId}:" +
                   $"school:{schoolId}:" +
                   $"page:{request.PageNumber}:" +
                   $"size:{request.PageSize}:" +
                   $"search:{request.SearchTerm?.ToLower() ?? "none"}:" +
                   $"active:{request.IsActive?.ToString() ?? "all"}:" +
                   $"system:{request.IsSystemRole?.ToString() ?? "all"}:" +
                   $"level:{request.Level?.ToString() ?? "all"}:" +
                   $"sort:{request.SortBy?.ToLower() ?? "default"}:" +
                   $"dir:{request.SortDirection?.ToLower() ?? "asc"}";
        }

        /// <summary>
        /// Fetch roles from database (called on cache miss)
        /// </summary>
        private async Task<PagedResult<RoleDto>> FetchRolesFromDatabase(
            GetAllRolesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("📊 Fetching roles from database (cache miss)");

            // Build query using GetQueryable
            var query = _unitOfWork.RoleRepository.GetQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    (r.DisplayName != null && r.DisplayName.ToLower().Contains(searchTerm)) ||
                    (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
            }

            // Apply active filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(r => r.IsActive == request.IsActive.Value);
            }

            // Apply system role filter
            if (request.IsSystemRole.HasValue)
            {
                query = query.Where(r => r.IsSystemRole == request.IsSystemRole.Value);
            }

            // Apply level filter
            if (request.Level.HasValue)
            {
                query = query.Where(r => r.Level == request.Level.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "displayname" => request.SortDirection == "desc"
                    ? query.OrderByDescending(r => r.DisplayName)
                    : query.OrderBy(r => r.DisplayName),
                "level" => request.SortDirection == "desc"
                    ? query.OrderByDescending(r => r.Level)
                    : query.OrderBy(r => r.Level),
                "createdat" => request.SortDirection == "desc"
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Name)
            };

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()  // ✅ Add for read-only performance
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var dtos = items.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                Level = role.Level
            }).ToList();

            // Create paged result
            return new PagedResult<RoleDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
