using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Sections.Commands;
using SchoolManagement.Application.Sections.Queries;
using SchoolManagement.Application.DTOs;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Section Management Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SectionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SectionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get paginated sections with optional filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="classId">Filter by class ID</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="searchTerm">Search in section name or room number</param>
        /// <param name="sortBy">Sort field: name, capacity, createdat, roomnumber (default: name)</param>
        /// <param name="sortDirection">Sort direction: asc or desc (default: asc)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSections(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? classId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            var query = new GetSectionsQuery(
                pageNumber,
                pageSize,
                classId,
                isActive,
                searchTerm,
                sortBy,
                sortDirection
            );

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get section by ID with complete details
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSectionById(Guid id)
        {
            var query = new GetSectionByIdQuery { SectionId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="command">Section creation details</param>
        /// <returns>Created section ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionCommand command)
        {
            var sectionId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetSectionById),
                new { id = sectionId },
                sectionId
            );
        }

        /// <summary>
        /// Update existing section
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <param name="command">Updated section details</param>
        /// <returns>Success response</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionCommand command)
        {
            if (id != command.Id)
                return BadRequest("Section ID mismatch");

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Delete a section (soft delete)
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            var command = new DeleteSectionCommand { Id = id };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Assign class teacher to a section
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <param name="request">Teacher assignment details</param>
        /// <returns>Success response</returns>
        [HttpPost("{id}/assign-teacher")]
        public async Task<IActionResult> AssignClassTeacher(Guid id, [FromBody] AssignClassTeacherCommand request)
        {
            var command = new AssignClassTeacherCommand
            {
                SectionId = id,
                TeacherId = request.TeacherId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Remove class teacher from a section
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}/remove-teacher")]
        public async Task<IActionResult> RemoveClassTeacher(Guid id)
        {
            var command = new RemoveClassTeacherCommand { SectionId = id };
            await _mediator.Send(command);
            return Ok();
        }
    }
}
