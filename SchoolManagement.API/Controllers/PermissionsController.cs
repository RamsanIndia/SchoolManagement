using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.Permissions.Commands;
using SchoolManagement.Application.Permissions.Queries;

namespace SchoolManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions(
        [FromQuery] string? searchTerm,
        [FromQuery] string? module,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] bool? isSystemPermission,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = new GetPermissionsQuery
            {
                SearchTerm = searchTerm,
                Module = module,
                Action = action,
                Resource = resource,
                IsSystemPermission = isSystemPermission,
                SortBy = sortBy,
                SortDirection = sortDirection,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PermissionDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var permission = await _mediator.Send(new GetPermissionByIdQuery { Id = id }, cancellationToken);
            if (permission == null) return NotFound();
            return Ok(permission);
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreatePermissionCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePermissionCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest(Result.Failure("ID mismatch", "The ID in the URL does not match the ID in the request body."));

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeletePermissionCommand { Id = id },
                cancellationToken
            );

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
