using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.AuditLogs.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;

namespace SchoolManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get audit logs with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Result<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PagedResult<AuditLogDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<PagedResult<AuditLogDto>>>> GetAuditLogs(
            [FromQuery] Guid? userId,
            [FromQuery] string entityName,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = new GetAuditLogsQuery
            {
                UserId = userId,
                EntityName = entityName,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get user activity history
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(Result<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PagedResult<AuditLogDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<PagedResult<AuditLogDto>>>> GetUserActivity(
            Guid userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = new GetUserActivityQuery
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get entity change history
        /// </summary>
        [HttpGet("entity/{entityName}/{entityId}")]
        [ProducesResponseType(typeof(Result<List<AuditLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<AuditLogDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<List<AuditLogDto>>>> GetEntityHistory(
            string entityName,
            string entityId)
        {
            var query = new GetEntityHistoryQuery
            {
                EntityName = entityName,
                EntityId = entityId
            };

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get recent activities
        /// </summary>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(Result<List<AuditLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<AuditLogDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<List<AuditLogDto>>>> GetRecentActivities(
            [FromQuery] int count = 10)
        {
            var query = new GetRecentActivitiesQuery
            {
                Count = count
            };

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
