using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Roles.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagement.Application.Roles.Handler.Queries
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<PagedResult<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResult<RoleDto>>> Handle(
            GetAllRolesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
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
                    _ => query.OrderBy(r => r.Name) // Default: sort by name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
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
                    // UserCount = role.UserCount // Uncomment if you add Include for users
                }).ToList();

                // Create paged result
                var response = new PagedResult<RoleDto>(
                    dtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result<PagedResult<RoleDto>>.Success(
                    response,
                    "Roles fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<PagedResult<RoleDto>>.Failure(
                    "Failed to fetch roles.",
                    ex.Message);
            }
        }
    }
}
