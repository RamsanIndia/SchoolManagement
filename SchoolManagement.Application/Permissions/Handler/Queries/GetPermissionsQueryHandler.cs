using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Permissions.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagement.Application.Permissions.Handler.Queries
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<PagedResult<PermissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPermissionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<PermissionDto>>> Handle(
            GetPermissionsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
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
                    _ => query.OrderBy(p => p.Name) // Default: sort by name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
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
                var response = new PagedResult<PermissionDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result<PagedResult<PermissionDto>>.Success(
                    response,
                    "Permissions fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<PagedResult<PermissionDto>>.Failure(
                    "Failed to fetch permissions.",
                    ex.Message);
            }
        }
    }
}
