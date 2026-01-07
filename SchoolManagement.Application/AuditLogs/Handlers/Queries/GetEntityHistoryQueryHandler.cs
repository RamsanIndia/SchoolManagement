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
    public class GetEntityHistoryQueryHandler
        : IRequestHandler<GetEntityHistoryQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetEntityHistoryQueryHandler> _logger;

        public GetEntityHistoryQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetEntityHistoryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<AuditLogDto>>> Handle(
            GetEntityHistoryQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.EntityName))
                {
                    return Result<List<AuditLogDto>>.Failure(
                        "Invalid entity name",
                        "Entity name cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(request.EntityId))
                {
                    return Result<List<AuditLogDto>>.Failure(
                        "Invalid entity ID",
                        "Entity ID cannot be empty");
                }

                var auditLogs = await _unitOfWork.AuditLogRepository.GetEntityHistoryAsync(
                    entityName: request.EntityName,
                    entityId: request.EntityId,
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
                }).ToList();

                return Result<List<AuditLogDto>>.Success(
                    auditLogDtos,
                    $"Successfully retrieved {auditLogDtos.Count} history record(s) for {request.EntityName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving entity history for {EntityName} with ID {EntityId}",
                    request.EntityName,
                    request.EntityId);
                return Result<List<AuditLogDto>>.FromException(
                    ex,
                    "Failed to retrieve entity history");
            }
        }
    }
}
