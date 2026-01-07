using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Behaviors
{
    public class ActivityLogBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ActivityLogBehavior<TRequest, TResponse>> _logger;
        private readonly SchoolManagementDbContext _context;

        public ActivityLogBehavior(
            ICurrentUserService currentUserService,
            ILogger<ActivityLogBehavior<TRequest, TResponse>> logger,
            SchoolManagementDbContext context)
        {
            _currentUserService = currentUserService;
            _logger = logger;
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[START] {RequestName} {RequestId} - User: {UserId}",
                requestName,
                requestId,
                _currentUserService.UserId);

            TResponse response;
            string status = "Success";
            string errorMessage = null;

            try
            {
                response = await next();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status = "Failed";
                errorMessage = ex.Message;

                _logger.LogError(ex,
                    "[FAILED] {RequestName} {RequestId} - Duration: {Duration}ms",
                    requestName,
                    requestId,
                    stopwatch.ElapsedMilliseconds);

                // Log the failed activity
                await LogActivityAsync(
                    requestName,
                    request,
                    stopwatch.ElapsedMilliseconds,
                    status,
                    errorMessage,
                    cancellationToken);

                throw;
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} {RequestId} - Duration: {Duration}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            // Log the successful activity
            await LogActivityAsync(
                requestName,
                request,
                stopwatch.ElapsedMilliseconds,
                status,
                null,
                cancellationToken);

            return response;
        }

        private async Task LogActivityAsync(
            string requestName,
            TRequest request,
            long duration,
            string status,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                var auditAction = DetermineAuditAction(requestName);
                var entityInfo = ExtractEntityInfo(request);

                var auditLog = AuditLog.Create(
                    action: auditAction,
                    entityName: entityInfo.EntityName ?? requestName,
                    entityId: entityInfo.EntityId ?? "N/A",
                    userId: _currentUserService.UserId,
                    userEmail: _currentUserService.Email,
                    ipAddress: _currentUserService.IpAddress,
                    userAgent: _currentUserService.UserAgent,
                    newValues: SerializeRequest(request),
                    duration: duration,
                    status: status);

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity for {RequestName}", requestName);
            }
        }

        private AuditAction DetermineAuditAction(string requestName)
        {
            if (requestName.Contains("Create", StringComparison.OrdinalIgnoreCase))
                return AuditAction.Create;
            if (requestName.Contains("Update", StringComparison.OrdinalIgnoreCase))
                return AuditAction.Update;
            if (requestName.Contains("Delete", StringComparison.OrdinalIgnoreCase))
                return AuditAction.Delete;
            if (requestName.Contains("Export", StringComparison.OrdinalIgnoreCase))
                return AuditAction.Export;
            if (requestName.Contains("Import", StringComparison.OrdinalIgnoreCase))
                return AuditAction.Import;

            return AuditAction.Read;
        }

        private (string EntityName, string EntityId) ExtractEntityInfo(TRequest request)
        {
            var requestType = request.GetType();

            // Try to get Id property
            var idProperty = requestType.GetProperty("Id") ??
                           requestType.GetProperty("EntityId");
            var entityId = idProperty?.GetValue(request)?.ToString();

            // Extract entity name from command/query name
            var entityName = requestType.Name
                .Replace("Command", "")
                .Replace("Query", "")
                .Replace("Create", "")
                .Replace("Update", "")
                .Replace("Delete", "")
                .Trim();

            return (entityName, entityId);
        }

        private string SerializeRequest(TRequest request)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Serialize(request, options);
            }
            catch
            {
                return "Unable to serialize request";
            }
        }
    }
}
