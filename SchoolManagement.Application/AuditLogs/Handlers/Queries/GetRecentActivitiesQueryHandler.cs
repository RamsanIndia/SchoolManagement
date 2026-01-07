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
    public class GetRecentActivitiesQueryHandler
        : IRequestHandler<GetRecentActivitiesQuery, Result<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetRecentActivitiesQueryHandler> _logger;

        public GetRecentActivitiesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetRecentActivitiesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<AuditLogDto>>> Handle(
            GetRecentActivitiesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.Count < 1 || request.Count > 100)
                {
                    return Result<List<AuditLogDto>>.Failure(
                        "Invalid count",
                        "Count must be between 1 and 100");
                }

                var auditLogs = await _unitOfWork.AuditLogRepository.GetRecentActivitiesAsync(
                    count: request.Count,
                    cancellationToken: cancellationToken);

                var auditLogDtos = auditLogs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action.ToString(),
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    UserEmail = a.UserEmail,
                    Timestamp = a.Timestamp,
                    Duration = a.Duration,
                    Status = a.Status
                }).ToList();

                return Result<List<AuditLogDto>>.Success(
                    auditLogDtos,
                    $"Successfully retrieved {auditLogDtos.Count} recent activit(ies)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities");
                return Result<List<AuditLogDto>>.FromException(
                    ex,
                    "Failed to retrieve recent activities");
            }
        }
    }
}
