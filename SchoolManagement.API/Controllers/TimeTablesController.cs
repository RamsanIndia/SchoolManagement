using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Application.TimeTables.Queries;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Timetable Management Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TimeTablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimeTablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get complete timetable for a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <returns>Weekly timetable</returns>
        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetSectionTimeTable(Guid sectionId)
        {
            var query = new GetTimeTableQuery { SectionId = sectionId };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get timetable for a teacher
        /// </summary>
        /// <param name="teacherId">Teacher ID</param>
        /// <returns>Teacher's weekly schedule</returns>
        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherTimeTable(Guid teacherId)
        {
            var query = new GetTeacherTimeTableQuery { TeacherId = teacherId };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Create a timetable entry
        /// </summary>
        /// <param name="command">Timetable entry details</param>
        /// <returns>Created entry ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTimeTableEntry([FromBody] CreateTimeTableEntryCommand command)
        {
            var entryId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetSectionTimeTable),
                new { sectionId = command.SectionId },
                entryId
            );
        }

        /// <summary>
        /// Update a timetable entry
        /// </summary>
        /// <param name="id">Entry ID</param>
        /// <param name="command">Updated entry details</param>
        /// <returns>Success response</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeTableEntry(
            Guid id,
            [FromBody] UpdateTimeTableEntryCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Delete a timetable entry
        /// </summary>
        /// <param name="id">Entry ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeTableEntry(Guid id)
        {
            var command = new DeleteTimeTableEntryCommand { Id = id };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Auto-generate timetable for a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Generation parameters</param>
        /// <returns>Generated timetable result</returns>
        [HttpPost("section/{sectionId}/generate")]
        public async Task<IActionResult> GenerateTimeTable(
            Guid sectionId,
            [FromBody] GenerateTimeTableCommand request)
        {
            var command = new GenerateTimeTableCommand
            {
                SectionId = sectionId,
                //StartDate = request.StartDate,
                //EndDate = request.EndDate,
                PeriodsPerDay = request.PeriodsPerDay,
                PeriodDuration = request.PeriodDuration,
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Check if a time slot is available for scheduling
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="teacherId">Teacher ID</param>
        /// <param name="dayOfWeek">Day of week</param>
        /// <param name="periodNumber">Period number</param>
        /// <returns>Availability information</returns>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckSlotAvailability(
            [FromQuery] Guid sectionId,
            [FromQuery] Guid teacherId,
            [FromQuery] DayOfWeek dayOfWeek,
            [FromQuery] int periodNumber)
        {
            var query = new CheckSlotAvailabilityQuery
            {
                SectionId = sectionId,
                TeacherId = teacherId,
                DayOfWeek = dayOfWeek,
                PeriodNumber = periodNumber
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
