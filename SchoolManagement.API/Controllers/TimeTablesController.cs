using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.TimeTables.Commands;
using SchoolManagement.Application.TimeTables.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Timetable Management API
    /// </summary>
    [ApiController]
    [Route("api/v1/timetables")]
    [Produces("application/json")]
    [Authorize]
    public class TimeTablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimeTablesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Get complete timetable for a section
        /// </summary>
        /// <param name="sectionId">Section unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Weekly timetable for the section</returns>
        /// <response code="200">Returns the section's timetable</response>
        /// <response code="404">Section not found or has no timetable</response>
        [HttpGet("sections/{sectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionTimeTable(
            Guid sectionId,
            CancellationToken cancellationToken)
        {
            var query = new GetTimeTableQuery { SectionId = sectionId };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || !result.Status)
                return NotFound(new { message = $"Timetable for section {sectionId} not found" });

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Get timetable for a specific day
        /// </summary>
        /// <param name="sectionId">Section unique identifier</param>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Timetable entries for the specified day</returns>
        /// <response code="200">Returns the day's schedule</response>
        /// <response code="404">No schedule found for the specified day</response>
        [HttpGet("sections/{sectionId}/days/{dayOfWeek}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSectionDaySchedule(
            Guid sectionId,
            DayOfWeek dayOfWeek,
            CancellationToken cancellationToken)
        {
            var query = new GetSectionDayScheduleQuery
            {
                SectionId = sectionId,
                DayOfWeek = dayOfWeek
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || !result.Status)
                return NotFound(new { message = $"No schedule found for {dayOfWeek}" });

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Get timetable for a teacher
        /// </summary>
        /// <param name="teacherId">Teacher unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Teacher's weekly schedule</returns>
        /// <response code="200">Returns the teacher's schedule</response>
        /// <response code="404">Teacher not found or has no schedule</response>
        [HttpGet("teachers/{teacherId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacherTimeTable(
            Guid teacherId,
            CancellationToken cancellationToken)
        {
            var query = new GetTeacherTimeTableQuery { TeacherId = teacherId };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || !result.Status)
                return NotFound(new { message = $"Schedule for teacher {teacherId} not found" });

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Get a specific timetable entry by ID
        /// </summary>
        /// <param name="id">Timetable entry unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Timetable entry details</returns>
        /// <response code="200">Returns the timetable entry</response>
        /// <response code="404">Entry not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTimeTableEntry(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetTimeTableEntryByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || !result.Status)
                return NotFound(new { message = $"Timetable entry {id} not found" });

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Create a new timetable entry
        /// </summary>
        /// <param name="command">Timetable entry details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created entry ID</returns>
        /// <response code="201">Entry created successfully</response>
        /// <response code="400">Invalid request data or business rule violation</response>
        /// <response code="409">Conflict - time slot already occupied</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateTimeTableEntry(
            [FromBody] CreateTimeTableEntryCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
                return BadRequest(new { errors = result.Errors, message = result.Message });

            return CreatedAtAction(
                nameof(GetTimeTableEntry),
                new { id = result.Data },
                new { id = result.Data, message = result.Message }
            );
        }

        /// <summary>
        /// Update an existing timetable entry
        /// </summary>
        /// <param name="id">Entry unique identifier</param>
        /// <param name="command">Updated entry details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response</returns>
        /// <response code="200">Entry updated successfully</response>
        /// <response code="400">Invalid request data or ID mismatch</response>
        /// <response code="404">Entry not found</response>
        /// <response code="409">Conflict - time slot already occupied</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateTimeTableEntry(
            Guid id,
            [FromBody] UpdateTimeTableEntryCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest(new { message = "URL ID does not match request body ID" });

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { errors = result.Errors, message = result.Message });

                return BadRequest(new { errors = result.Errors, message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Delete a timetable entry (soft delete)
        /// </summary>
        /// <param name="id">Entry unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response</returns>
        /// <response code="200">Entry deleted successfully</response>
        /// <response code="404">Entry not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTimeTableEntry(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteTimeTableEntryCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { message = result.Message });

                return BadRequest(new { errors = result.Errors, message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Auto-generate timetable for a section
        /// </summary>
        /// <param name="sectionId">Section unique identifier</param>
        /// <param name="request">Generation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generation result with statistics</returns>
        /// <response code="200">Timetable generated successfully</response>
        /// <response code="400">Invalid parameters or prerequisites not met</response>
        /// <response code="404">Section not found</response>
        [HttpPost("sections/{sectionId}/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateTimeTable(
            Guid sectionId,
            [FromBody] GenerateTimeTableRequest request,
            CancellationToken cancellationToken)
        {
            var command = new GenerateTimeTableCommand(
                sectionId,
                request.PeriodsPerDay,
                request.PeriodDuration,
                request.BreakAfterPeriod,
                request.BreakDuration,
                request.SchoolStartTime,
                request.OverwriteExisting,
                request.WorkingDays
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { errors = result.Errors, message = result.Message });

                return BadRequest(new { errors = result.Errors, message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Check if a time slot is available for scheduling
        /// </summary>
        /// <param name="sectionId">Section unique identifier</param>
        /// <param name="teacherId">Teacher unique identifier</param>
        /// <param name="roomNumber">Room number</param>
        /// <param name="dayOfWeek">Day of week</param>
        /// <param name="periodNumber">Period number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Availability information with conflict details</returns>
        /// <response code="200">Returns availability status</response>
        [HttpGet("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckSlotAvailability(
            [FromQuery] Guid sectionId,
            [FromQuery] Guid teacherId,
            [FromQuery] string roomNumber,
            [FromQuery] DayOfWeek dayOfWeek,
            [FromQuery] int periodNumber,
            CancellationToken cancellationToken)
        {
            var query = new CheckSlotAvailabilityQuery
            {
                SectionId = sectionId,
                TeacherId = teacherId,
                RoomNumber = roomNumber,
                DayOfWeek = dayOfWeek,
                PeriodNumber = periodNumber
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(new { data = result.Data });
        }

        /// <summary>
        /// Bulk delete timetable entries for a section
        /// </summary>
        /// <param name="sectionId">Section unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deletion result</returns>
        /// <response code="200">Entries deleted successfully</response>
        /// <response code="404">Section not found</response>
        [HttpDelete("sections/{sectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSectionTimeTable(
            Guid sectionId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteSectionTimeTableCommand { SectionId = sectionId };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Status)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { message = result.Message });

                return BadRequest(new { errors = result.Errors, message = result.Message });
            }

            return Ok(new { message = result.Message, deletedCount = result.Data });
        }
    }

    /// <summary>
    /// Request model for timetable generation
    /// </summary>
    public class GenerateTimeTableRequest
    {
        /// <summary>
        /// Number of periods per day (1-10)
        /// </summary>
        public int PeriodsPerDay { get; set; } = 8;

        /// <summary>
        /// Duration of each period in minutes (30-120)
        /// </summary>
        public int PeriodDuration { get; set; } = 45;

        /// <summary>
        /// Period number after which break occurs
        /// </summary>
        public int BreakAfterPeriod { get; set; } = 4;

        /// <summary>
        /// Break duration in minutes (15-60)
        /// </summary>
        public int BreakDuration { get; set; } = 30;

        /// <summary>
        /// School start time (default: 08:00 AM)
        /// </summary>
        public TimeSpan SchoolStartTime { get; set; } = TimeSpan.FromHours(8);

        /// <summary>
        /// Whether to overwrite existing timetable entries
        /// </summary>
        public bool OverwriteExisting { get; set; } = false;

        /// <summary>
        /// Working days (default: Monday-Saturday)
        /// </summary>
        public DayOfWeek[] WorkingDays { get; set; } = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };
    }
}
