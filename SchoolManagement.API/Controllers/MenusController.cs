using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Helpers;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Application.Menus.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MenusController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMenuPermissionService _menuPermissionService;

        public MenusController(IMediator mediator, IMenuPermissionService menuPermissionService)
        {
            _mediator = mediator;
            _menuPermissionService = menuPermissionService;
        }

        /// <summary>
        /// Get user's accessible menus with permissions
        /// </summary>
        [HttpGet("user-menus")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetUserMenus(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var menus = await _menuPermissionService.GetUserMenusAsync(userId);
            return Ok(menus);
        }

        /// <summary>
        /// Get all menus (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> GetAllMenus(CancellationToken cancellationToken)
        {
            var query = new GetAllMenusQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get menu hierarchy
        /// </summary>
        [HttpGet("hierarchy")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> GetMenuHierarchy(CancellationToken cancellationToken)
        {
            var query = new GetMenuHierarchyQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get menu by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> GetMenu(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetMenuByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get menus by role ID
        /// </summary>
        [HttpGet("role/{roleId:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> GetMenusByRole(Guid roleId, CancellationToken cancellationToken)
        {
            var query = new GetMenusByRoleQuery(roleId.ToString());
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Create new menu
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> CreateMenu([FromBody] CreateMenuCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetMenu),
                new { id = result.Data.Id },
                result
            );
        }

        /// <summary>
        /// Update menu
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest(Result.Failure("ID mismatch", "The ID in the URL does not match the ID in the request body."));

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete menu
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> DeleteMenu(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteMenuCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Check user's access to specific menu
        /// </summary>
        [HttpGet("{menuId:guid}/access")]
        public async Task<ActionResult<MenuAccessDto>> CheckMenuAccess(Guid menuId)
        {
            var userId = GetCurrentUserId();
            var hasAccess = await _menuPermissionService.HasMenuAccessAsync(userId, menuId);
            var permissions = await _menuPermissionService.GetUserMenuPermissionsAsync(userId, menuId);

            return Ok(new MenuAccessDto
            {
                MenuId = menuId,
                HasAccess = hasAccess,
                Permissions = permissions
            });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User not authenticated"));
        }
    }
}