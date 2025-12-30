using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Teachers.Commands;
using SchoolManagement.Application.Teachers.Queries;
using SchoolManagement.Domain.Common;

namespace SchoolManagement.API.Controllers
{
    /// <summary>
    /// Manages teacher-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class TeachersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(IMediator mediator, ILogger<TeachersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new teacher
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherCommand command)
        {
            // Input validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating teacher with Employee ID: {EmployeeId}", command.EmployeeId);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets a teacher by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacherById(Guid id)
        {
            _logger.LogInformation("Retrieving teacher with ID: {TeacherId}", id);

            var query = new GetTeacherByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get paginated teachers with optional filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search in name, employee ID, email, or phone</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="departmentId">Filter by department</param>
        /// <param name="sortBy">Sort field: firstname, lastname, employeeid, dateofjoining, experience (default: lastname)</param>
        /// <param name="sortDirection">Sort direction: asc or desc (default: asc)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllTeachers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] Guid? departmentId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            var query = new GetAllTeachersQuery(
                pageNumber,
                pageSize,
                searchTerm,
                isActive,
                departmentId,
                sortBy,
                sortDirection
            );

            var result = await _mediator.Send(query);

            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        ///// <summary>
        ///// Gets teachers by department
        ///// </summary>
        //[HttpGet("department/{departmentId:guid}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetTeachersByDepartment(
        //    Guid departmentId,
        //    [FromQuery] bool activeOnly = true)
        //{
        //    _logger.LogInformation(
        //        "Retrieving teachers for department: {DepartmentId}, ActiveOnly: {ActiveOnly}",
        //        departmentId, activeOnly);

        //    var query = new GetTeachersByDepartmentQuery
        //    {
        //        DepartmentId = departmentId,
        //        ActiveOnly = activeOnly
        //    };

        //    var result = await _mediator.Send(query);
        //    return Ok(result);
        //}

        /// <summary>
        /// Get detailed workload information for a specific teacher
        /// </summary>
        /// <param name="teacherId">Teacher ID</param>
        /// <returns>Teacher workload details including assignments and class teacher responsibilities</returns>
        [HttpGet("{teacherId}/workload")]
        [ProducesResponseType(typeof(Result<TeacherWorkloadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTeacherWorkload([FromRoute] Guid teacherId)
        {
            var query = new GetTeacherWorkloadQuery(teacherId);
            var result = await _mediator.Send(query);

            if (!result.Status)
            {
                if (result.Message == "TeacherNotFound")
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates teacher's personal details
        /// </summary>
        [HttpPut("{id:guid}/personal-details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePersonalDetails(
            Guid id,
            [FromBody] UpdateTeacherPersonalDetailsCommand command)
        {
            // Input validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Route parameter validation
            if (id != command.TeacherId)
            {
                return BadRequest(new { Message = "Teacher ID in URL does not match request body" });
            }

            _logger.LogInformation("Updating personal details for teacher: {TeacherId}", id);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates teacher's professional details
        /// </summary>
        [HttpPut("{id:guid}/professional-details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfessionalDetails(
            Guid id,
            [FromBody] UpdateTeacherProfessionalDetailsCommand command)
        {
            // Input validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Route parameter validation
            if (id != command.TeacherId)
            {
                return BadRequest(new { Message = "Teacher ID in URL does not match request body" });
            }

            _logger.LogInformation("Updating professional details for teacher: {TeacherId}", id);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Activates an inactive teacher
        /// </summary>
        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateTeacher(Guid id)
        {
            _logger.LogInformation("Activating teacher: {TeacherId}", id);

            var command = new ActivateTeacherCommand { TeacherId = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Deactivates an active teacher
        /// </summary>
        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeactivateTeacher(Guid id, [FromQuery] DateTime? leavingDate = null)
        {
            _logger.LogInformation("Deactivating teacher: {TeacherId}", id);

            var command = new DeactivateTeacherCommand
            {
                TeacherId = id,
                LeavingDate = leavingDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a teacher (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteTeacher(Guid id)
        {
            _logger.LogInformation("Deleting teacher: {TeacherId}", id);

            var command = new DeactivateTeacherCommand { TeacherId = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}

