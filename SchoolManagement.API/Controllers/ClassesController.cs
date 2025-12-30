using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Classes.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Threading.Tasks;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Class Management Controller - Handles CRUD operations for classes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // ✅ Add authorization at controller level
    public class ClassesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get paginated classes with optional filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search in class name, code, or description</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="academicYearId">Filter by academic year</param>
        /// <param name="sortBy">Sort field: name, code, grade, createdat (default: name)</param>
        /// <param name="sortDirection">Sort direction: asc or desc (default: asc)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClasses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] Guid? academicYearId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            var query = new GetClassesQuery(
                pageNumber,
                pageSize,
                searchTerm,
                isActive,
                academicYearId,
                sortBy,
                sortDirection
            );

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get class by ID with complete details
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Class details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<ClassDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result<ClassDto>>> GetClassById(Guid id)
        {
            var query = new GetClassByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a new class
        /// </summary>
        /// <param name="command">Class creation details</param>
        /// <returns>Created class details</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")] // ✅ Role-based authorization
        [ProducesResponseType(typeof(Result<ClassDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<ClassDto>>> CreateClass(
            [FromBody] CreateClassCommand command)
        {
            // ✅ Optional: Set CreatedBy from authenticated user
            // command.CreatedBy = User.Identity.Name;

            var result = await _mediator.Send(command);

            if (!result.Status)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetClassById),
                new { id = result.Data.Id },
                result);
        }

        /// <summary>
        /// Update existing class
        /// </summary>
        /// <param name="id">Class ID from route</param>
        /// <param name="command">Updated class details</param>
        /// <returns>Updated class details</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(Result<ClassDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result<ClassDto>>> UpdateClass(
            Guid id,
            [FromBody] UpdateClassCommand command)
        {
            if (id != command.Id)
                return BadRequest(Result<ClassDto>.Failure("Route ID and command ID mismatch."));

            var result = await _mediator.Send(command);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a class (soft delete)
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result>> DeleteClass(Guid id)
        {
            var command = new DeleteClassCommand { Id = id };

            var result = await _mediator.Send(command);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Activate a class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result>> ActivateClass(Guid id)
        {
            var command = new ActivateClassCommand { ClassId = id };

            var result = await _mediator.Send(command);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Deactivate a class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result>> DeactivateClass(Guid id)
        {
            var command = new DeactivateClassCommand { ClassId = id };

            var result = await _mediator.Send(command);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }
    }
}
