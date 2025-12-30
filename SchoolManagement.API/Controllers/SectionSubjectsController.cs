using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.SectionSubjects.Commands;
using SchoolManagement.Application.SectionSubjects.Queries;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Subject Mapping Controller
    /// </summary>
    [ApiController]
    [Route("api/sections/{sectionId}/subjects")]
    [Produces("application/json")]
    public class SectionSubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SectionSubjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all subjects mapped to a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>Paginated list of mapped subjects</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSectionSubjects(
            [FromRoute] Guid sectionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isMandatory = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            // Constructor applies validation automatically
            var query = new GetSectionSubjectsQuery(
                sectionId,
                pageNumber,
                pageSize,
                searchTerm,
                isMandatory,
                sortBy,
                sortDirection
            );

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }


        /// <summary>
        /// Map a subject to a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Subject mapping details</param>
        /// <returns>Created mapping ID</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MapSubject(
            [FromRoute] Guid sectionId,
            [FromBody] MapSubjectCommand request)
        {
            // Set sectionId from route parameter
            request.SectionId = sectionId;

            var result = await _mediator.Send(request);

            if (!result.Status)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetSectionSubjects),
                new { sectionId },
                result
            );
        }

        /// <summary>
        /// Update subject mapping (change teacher or weekly periods)
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="mappingId">Mapping ID</param>
        /// <param name="request">Updated mapping details</param>
        /// <returns>Success response</returns>
        [HttpPut("{mappingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubjectMapping(
            [FromRoute] Guid sectionId,
            [FromRoute] Guid mappingId,
            [FromBody] UpdateSubjectMappingCommand request)
        {
            // Set mappingId from route parameter
            request.MappingId = mappingId;

            var result = await _mediator.Send(request);

            if (!result.Status)
                return result.Message.Contains("not found")
                    ? NotFound(result)
                    : BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Remove subject mapping from section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="mappingId">Mapping ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{mappingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveSubjectMapping(
            [FromRoute] Guid sectionId,
            [FromRoute] Guid mappingId)
        {
            var command = new RemoveSubjectMappingCommand
            {
                MappingId = mappingId,
                SectionId = sectionId  // Optional: for additional validation
            };

            var result = await _mediator.Send(command);

            if (!result.Status)
                return result.Message.Contains("not found")
                    ? NotFound(result)
                    : BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Bulk map multiple subjects to a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Bulk mapping request</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkMapSubjects(
            [FromRoute] Guid sectionId,
            [FromBody] BulkMapSubjectsCommand request)
        {
            // Set sectionId from route parameter
            request.SectionId = sectionId;

            var result = await _mediator.Send(request);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
