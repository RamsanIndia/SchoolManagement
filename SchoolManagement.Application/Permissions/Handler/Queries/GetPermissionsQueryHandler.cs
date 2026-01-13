using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Permissions.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Permissions.Handler.Queries
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<PagedResult<PermissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<GetPermissionsQueryHandler> _logger;

        public GetPermissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ITenantService tenantService,
            ILogger<GetPermissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<Result<PagedResult<PermissionDto>>> Handle(
            GetPermissionsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Generate cache key based on query parameters and tenant context
                var cacheKey = GenerateCacheKey(request);

                _logger.LogDebug(
                    "Fetching permissions - PageNumber: {PageNumber}, PageSize: {PageSize}, CacheKey: {CacheKey}",
                    request.PageNumber, request.PageSize, cacheKey);

                // ✅ Use cache with 60-minute expiration (permissions change very rarely)
                var result = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () => await FetchPermissionsFromDatabase(request, cancellationToken),
                    absoluteExpiration: TimeSpan.FromHours(1),
                    slidingExpiration: TimeSpan.FromMinutes(30),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation(
                    "✅ Successfully fetched {Count} permissions out of {Total} (Page {Page}/{TotalPages})",
                    result.Items.Count(),
                    result.TotalCount,
                    request.PageNumber,
                    result.TotalPages);

                return Result<PagedResult<PermissionDto>>.Success(
                    result,
                    "Permissions fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error fetching permissions");
                return Result<PagedResult<PermissionDto>>.Failure(
                    "Failed to fetch permissions.",
                    ex.Message);
            }
        }

        /// <summary>
        /// Generate unique cache key based on query parameters and tenant context
        /// </summary>
        private string GenerateCacheKey(GetPermissionsQuery request)
        {
            var tenantId = _tenantService.TenantId;
            var schoolId = _tenantService.SchoolId ?? Guid.Empty;

            // ✅ Include all filter parameters in cache key
            return $"permissions:" +
                   $"tenant:{tenantId}:" +
                   $"school:{schoolId}:" +
                   $"page:{request.PageNumber}:" +
                   $"size:{request.PageSize}:" +
                   $"search:{request.SearchTerm?.ToLower() ?? "none"}:" +
                   $"module:{request.Module?.ToLower() ?? "all"}:" +
                   $"action:{request.Action?.ToLower() ?? "all"}:" +
                   $"resource:{request.Resource?.ToLower() ?? "all"}:" +
                   $"system:{request.IsSystemPermission?.ToString() ?? "all"}:" +
                   $"sort:{request.SortBy?.ToLower() ?? "default"}:" +
                   $"dir:{request.SortDirection?.ToLower() ?? "asc"}";
        }

        /// <summary>
        /// Fetch permissions from database (called on cache miss)
        /// </summary>
        private async Task<PagedResult<PermissionDto>> FetchPermissionsFromDatabase(
            GetPermissionsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("📊 Fetching permissions from database (cache miss)");

            // Build query using GetQueryable
            var query = _unitOfWork.Permissions.GetQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.DisplayName != null && p.DisplayName.ToLower().Contains(searchTerm)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            // Apply module filter
            if (!string.IsNullOrWhiteSpace(request.Module))
            {
                query = query.Where(p => p.Module == request.Module);
            }

            // Apply action filter
            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                query = query.Where(p => p.Action == request.Action);
            }

            // Apply resource filter
            if (!string.IsNullOrWhiteSpace(request.Resource))
            {
                query = query.Where(p => p.Resource == request.Resource);
            }

            // Apply system permission filter
            if (request.IsSystemPermission.HasValue)
            {
                query = query.Where(p => p.IsSystemPermission == request.IsSystemPermission.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "displayname" => request.SortDirection == "desc"
                    ? query.OrderByDescending(p => p.DisplayName)
                    : query.OrderBy(p => p.DisplayName),
                "module" => request.SortDirection == "desc"
                    ? query.OrderByDescending(p => p.Module)
                    : query.OrderBy(p => p.Module),
                "action" => request.SortDirection == "desc"
                    ? query.OrderByDescending(p => p.Action)
                    : query.OrderBy(p => p.Action),
                "resource" => request.SortDirection == "desc"
                    ? query.OrderByDescending(p => p.Resource)
                    : query.OrderBy(p => p.Resource),
                "createdat" => request.SortDirection == "desc"
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
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
            var dtos = items.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Module = p.Module,
                Action = p.Action,
                Resource = p.Resource,
                Description = p.Description,
                IsSystemPermission = p.IsSystemPermission
            }).ToList();

            // Create paged result
            return new PagedResult<PermissionDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
