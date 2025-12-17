using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using SchoolManagement.Application.UserRoles.Commands;
using SchoolManagement.Application.UserRoles.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin,HRManager")]
    public class UserRolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserRolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get user's roles
        /// </summary>
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var query = new GetUserRolesQuery { UserId = userId };
            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(new { result.Errors });

            return Ok(result);
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, AssignRoleToUserCommand command)
        {
            command.UserId = userId;
            var result = await _mediator.Send(command);

            // If you refactor AssignRoleToUserCommandHandler to return Result<bool>
            if (!result.Status)
                return BadRequest(new { result.Errors });

            return Ok(result);
        }

        /// <summary>
        /// Revoke role from user
        /// </summary>
        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<IActionResult> RevokeRoleFromUser(Guid userId, Guid roleId)
        {
            var command = new RevokeRoleFromUserCommand { UserId = userId, RoleId = roleId };
            var result = await _mediator.Send(command);

            if (!result.Status)
                return BadRequest(new { result.Errors });

            return Ok(result);
        }

        /// <summary>
        /// Update user role (extend expiry, activate/deactivate)
        /// </summary>
        [HttpPut("{userId}/roles/{roleId}")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, Guid roleId, UpdateUserRoleCommand command)
        {
            command.UserId = userId;
            command.RoleId = roleId;
            var result = await _mediator.Send(command);

            if (!result.Status)
                return BadRequest(new { result.Errors });

            return Ok(result);
        }
    }
}
