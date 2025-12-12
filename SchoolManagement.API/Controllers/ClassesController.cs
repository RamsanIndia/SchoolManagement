using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Classes.Queries;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Class Management Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClassesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get paginated list of classes with filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search term for filtering by name or code</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>Paginated list of classes</returns>
        [HttpGet]
        //[ProducesResponseType(typeof(ApiResponse<PagedResult<ClassListDto>>), 200)]
        public async Task<IActionResult> GetClasses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetClassesQuery
            {
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100), // Max 100 items per page
                SearchTerm = searchTerm,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok();
        }

        /// <summary>
        /// Get class by ID with complete details
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Class details</returns>
        [HttpGet("{id}")]
        //[ProducesResponseType(typeof(ApiResponse<ClassDetailDto>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<ClassDetailDto>), 404)]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var query = new GetClassByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// Create a new class
        /// </summary>
        /// <param name="command">Class creation details</param>
        /// <returns>Created class ID</returns>
        [HttpPost]
        //[ProducesResponseType(typeof(ApiResponse<Guid>), 201)]
        //[ProducesResponseType(typeof(ApiResponse<Guid>), 400)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassCommand command)
        {
            var classId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetClassById),
                new { id = classId }
                //ApiResponse<Guid>.SuccessResponse(classId, "Class created successfully")
            );
        }

        /// <summary>
        /// Update existing class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <param name="command">Updated class details</param>
        /// <returns>Success response</returns>
        [HttpPut("{id}")]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 400)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> UpdateClass(Guid id, [FromBody] UpdateClassCommand command)
        {
            if (id != command.Id)
                return BadRequest();

            await _mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Delete a class (soft delete)
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 400)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            var command = new DeleteClassCommand
            {
                Id = id,
            };

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Activate a class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpPatch("{id}/activate")]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> ActivateClass(Guid id)
        {
            var command = new ActivateClassCommand
            {
                Id = id,
            };

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Deactivate a class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success response</returns>
        [HttpPatch("{id}/deactivate")]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> DeactivateClass(Guid id)
        {
            var command = new DeactivateClassCommand
            {
                Id = id,
            };

            await _mediator.Send(command);
            return Ok();
        }
    }
}
