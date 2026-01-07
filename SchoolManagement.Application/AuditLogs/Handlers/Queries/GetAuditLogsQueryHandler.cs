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
    public class GetAuditLogsQueryHandler
        : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAuditLogsQueryHandler> _logger;

        public GetAuditLogsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetAuditLogsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PagedResult<AuditLogDto>>> Handle(
            GetAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate request
                if (request.PageNumber < 1)
                {
                    return Result<PagedResult<AuditLogDto>>.Failure(
                        "Invalid page number",
                        "Page number must be greater than 0");
                }

                if (request.PageSize < 1 || request.PageSize > 100)
                {
                    return Result<PagedResult<AuditLogDto>>.Failure(
                        "Invalid page size",
                        "Page size must be between 1 and 100");
                }

                if (request.StartDate.HasValue && request.EndDate.HasValue
                    && request.StartDate > request.EndDate)
                {
                    return Result<PagedResult<AuditLogDto>>.Failure(
                        "Invalid date range",
                        "Start date cannot be greater than end date");
                }

                // Get audit logs from repository
                var auditLogs = await _unitOfWork.AuditLogRepository.GetAuditLogsAsync(
                    userId: request.UserId,
                    entityName: request.EntityName,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken);

                // Get total count for pagination
                var totalCount = await _unitOfWork.AuditLogRepository.GetAuditLogsCountAsync(
                    userId: request.UserId,
                    entityName: request.EntityName,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    cancellationToken: cancellationToken);

                // Map to DTOs
                var auditLogDtos = auditLogs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action.ToString(),
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    UserEmail = a.UserEmail,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Timestamp = a.Timestamp,
                    OldValues = string.IsNullOrWhiteSpace(a.OldValues)
                                 ? new Dictionary<string, object>()
                                 : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.OldValues),
                                    NewValues = string.IsNullOrWhiteSpace(a.NewValues)
                                        ? new Dictionary<string, object>()
                                        : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.NewValues),
                    ChangedFields = string.IsNullOrWhiteSpace(a.ChangedFields) ? new List<string>() : a.ChangedFields.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList(),
                    Duration = a.Duration,
                    Status = a.Status
                });

                var pagedResult = new PagedResult<AuditLogDto>(
                    auditLogDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);

                _logger.LogInformation(
                    "Retrieved {Count} audit logs (Page {PageNumber}/{TotalPages})",
                    auditLogDtos.Count(),
                    pagedResult.PageNumber,
                    pagedResult.TotalPages);

                return Result<PagedResult<AuditLogDto>>.Success(
                    pagedResult,
                    $"Successfully retrieved {totalCount} audit log(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return Result<PagedResult<AuditLogDto>>.FromException(
                    ex,
                    "Failed to retrieve audit logs");
            }
        }
    }
}
