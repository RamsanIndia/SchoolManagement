using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.AcademicYears.Commands;
using SchoolManagement.Application.AcademicYears.Queries;
using SchoolManagement.Application.DTOs;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Manages academic year operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AcademicYearsController> _logger;

        public AcademicYearsController(IMediator mediator, ILogger<AcademicYearsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new academic year
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating academic year: {Name}", request.Name);

            var command = new CreateAcademicYearCommand
            {
                Name = request.Name,
                StartYear = request.StartYear,
                EndYear = request.EndYear,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets an academic year by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAcademicYearById(Guid id)
        {
            _logger.LogInformation("Retrieving academic year: {Id}", id);

            var query = new GetAcademicYearByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Gets all academic years
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAcademicYears([FromQuery] bool activeOnly = true)
        {
            _logger.LogInformation("Retrieving all academic years. ActiveOnly: {ActiveOnly}", activeOnly);

            var query = new GetAllAcademicYearsQuery { ActiveOnly = activeOnly };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Gets the current academic year
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentAcademicYear()
        {
            _logger.LogInformation("Retrieving current academic year");

            var query = new GetCurrentAcademicYearQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Updates an academic year
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAcademicYear(Guid id, [FromBody] UpdateAcademicYearRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest(new { Message = "ID in URL does not match request body" });
            }

            _logger.LogInformation("Updating academic year: {Id}", id);

            var command = new UpdateAcademicYearCommand
            {
                Id = request.Id,
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Sets an academic year as current
        /// </summary>
        [HttpPatch("{id:guid}/set-current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetCurrentAcademicYear(Guid id)
        {
            _logger.LogInformation("Setting academic year as current: {Id}", id);

            var command = new SetCurrentAcademicYearCommand { Id = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Activates an academic year
        /// </summary>
        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateAcademicYear(Guid id)
        {
            _logger.LogInformation("Activating academic year: {Id}", id);

            var command = new ActivateAcademicYearCommand { Id = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Deactivates an academic year
        /// </summary>
        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeactivateAcademicYear(Guid id)
        {
            _logger.LogInformation("Deactivating academic year: {Id}", id);

            var command = new DeactivateAcademicYearCommand { Id = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Deletes an academic year (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAcademicYear(Guid id)
        {
            _logger.LogInformation("Deleting academic year: {Id}", id);

            var command = new DeleteAcademicYearCommand { Id = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
