using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Application.Models;
using SchoolManagement.Application.Roles.Commands;
using SchoolManagement.Application.Roles.Queries;

namespace SchoolManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create new role
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<RoleDto>>> CreateRole(CreateRoleCommand command)
        {
            var response = await _mediator.Send(command);

            if (response.Status)
            {
                return CreatedAtAction(nameof(GetRole), new { id = response.Data }, response);
            }

            return BadRequest(response);
        }


        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var query = new GetAllRolesQuery();
            var roles = await _mediator.Send(query);
            return Ok(roles);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(Guid id)
        {
            var query = new GetRoleByIdQuery(id); 
            var role = await _mediator.Send(query);

            if (role == null)
                return NotFound();

            return Ok(role);
        }

        /// <summary>
        /// Assign menu permissions to role
        /// </summary>
        [HttpPost("{roleId}/menu-permissions")]
        public async Task<ActionResult<Result<bool>>> AssignMenuPermissions(Guid roleId, [FromBody] AssignMenuPermissionsCommand command)
        {
            command.RoleId = roleId;

            var response = await _mediator.Send(command);

            if (response.Status)
                return Ok(response);

            return BadRequest(response);
        }


        /// <summary>
        /// Update role
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<bool>>> UpdateRole(Guid id, UpdateRoleCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);

            if (response.Status)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Delete role
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteRole(Guid id)
        {
            var command = new DeleteRoleCommand { Id = id };
            var response = await _mediator.Send(command);

            if (response.Status)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
