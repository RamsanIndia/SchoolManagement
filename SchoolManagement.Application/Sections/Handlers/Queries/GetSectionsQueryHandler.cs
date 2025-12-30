using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Sections.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Sections.Handlers.Queries
{
    public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, Result<PagedResult<SectionListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetSectionsQueryHandler> _logger;

        public GetSectionsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetSectionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<PagedResult<SectionListDto>>> Handle(
            GetSectionsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving sections - Page: {PageNumber}, Size: {PageSize}, ClassId: {ClassId}, IsActive: {IsActive}",
                    request.PageNumber,
                    request.PageSize,
                    request.ClassId,
                    request.IsActive);

                // Build query using GetQueryable
                var query = _unitOfWork.SectionsRepository.GetQueryable();

                // Apply class filter
                if (request.ClassId.HasValue)
                {
                    query = query.Where(s => s.ClassId == request.ClassId.Value);
                }

                // Apply active filter
                if (request.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == request.IsActive.Value);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(searchTerm) ||
                        s.RoomNumber.Value.ToLower().Contains(searchTerm));
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "capacity" => request.SortDirection == "desc"
                        ? query.OrderByDescending(s => s.Capacity.MaxCapacity)
                        : query.OrderBy(s => s.Capacity.MaxCapacity),
                    "createdat" => request.SortDirection == "desc"
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt),
                    "roomnumber" => request.SortDirection == "desc"
                        ? query.OrderByDescending(s => s.RoomNumber.Value)
                        : query.OrderBy(s => s.RoomNumber.Value),
                    _ => query.OrderBy(s => s.Name) // Default: sort by name
                };

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and map to DTOs
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(s => new SectionListDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ClassId = s.ClassId,
                        ClassName = s.Class.Name,
                        MaxCapacity = s.Capacity.MaxCapacity,
                        CurrentStrength = s.Capacity.CurrentStrength,
                        RoomNumber = s.RoomNumber.Value,
                        IsActive = s.IsActive
                    })
                    .ToListAsync(cancellationToken);

                // Create paged result
                var pagedResult = new PagedResult<SectionListDto>(
                    items,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);

                _logger.LogInformation(
                    "Successfully retrieved {Count} sections out of {TotalCount}",
                    items.Count,
                    totalCount);

                return Result<PagedResult<SectionListDto>>.Success(
                    pagedResult,
                    "Sections retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sections");
                return Result<PagedResult<SectionListDto>>.Failure(
                    "An error occurred while retrieving sections.",
                    ex.Message);
            }
        }
    }
}
