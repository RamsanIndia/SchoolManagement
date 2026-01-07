using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.AuditLogs.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AuditLogs.Handlers.Queries
{
    public class GetUserActivityQueryHandler
        : IRequestHandler<GetUserActivityQuery, Result<PagedResult<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUserActivityQueryHandler> _logger;

        public GetUserActivityQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetUserActivityQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PagedResult<AuditLogDto>>> Handle(
            GetUserActivityQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.UserId == Guid.Empty)
                {
                    return Result<PagedResult<AuditLogDto>>.Failure(
                        "Invalid user ID",
                        "User ID cannot be empty");
                }

                var auditLogs = await _unitOfWork.AuditLogRepository.GetUserActivityAsync(
                    userId: request.UserId,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken);

                var totalCount = await _unitOfWork.AuditLogRepository.GetAuditLogsCountAsync(
                    userId: request.UserId,
                    entityName: null,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    cancellationToken: cancellationToken);

                var auditLogDtos = auditLogs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action.ToString(),
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    UserEmail = a.UserEmail,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    // ChangedFields expects List<string>, but a.ChangedFields is string.
                    // We'll split the string by comma, trim, and filter out empty entries.
                    ChangedFields = string.IsNullOrWhiteSpace(a.ChangedFields)
                        ? new List<string>()
                        : a.ChangedFields.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .Where(f => !string.IsNullOrEmpty(f))
                            .ToList(),
                    Duration = a.Duration,
                    Status = a.Status
                });

                var pagedResult = new PagedResult<AuditLogDto>(
                    auditLogDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);

                return Result<PagedResult<AuditLogDto>>.Success(
                    pagedResult,
                    $"Successfully retrieved user activity");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity for UserId: {UserId}", request.UserId);
                return Result<PagedResult<AuditLogDto>>.FromException(
                    ex,
                    "Failed to retrieve user activity");
            }
        }
    }
}
