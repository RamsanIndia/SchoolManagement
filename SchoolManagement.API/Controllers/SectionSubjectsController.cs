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
        /// <returns>List of mapped subjects</returns>
        [HttpGet]
        //[ProducesResponseType(typeof(ApiResponse<List<SectionSubjectDto>>), 200)]
        public async Task<IActionResult> GetSectionSubjects(Guid sectionId)
        {
            var query = new GetSectionSubjectsQuery { SectionId = sectionId };
            var result = await _mediator.Send(query);

            return Ok();
        }

        /// <summary>
        /// Map a subject to a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Subject mapping details</param>
        /// <returns>Created mapping ID</returns>
        [HttpPost]
        //[ProducesResponseType(typeof(ApiResponse<Guid>), 201)]
        //[ProducesResponseType(typeof(ApiResponse<Guid>), 400)]
        public async Task<IActionResult> MapSubject(Guid sectionId, [FromBody] MapSubjectCommand request)
        {
            var command = new MapSubjectCommand
            {
                SectionId = sectionId,
                SubjectId = request.SubjectId,
                SubjectName = request.SubjectName,
                SubjectCode = request.SubjectCode,
                TeacherId = request.TeacherId,
                TeacherName = request.TeacherName,
                WeeklyPeriods = request.WeeklyPeriods,
                IsMandatory = request.IsMandatory,
            };

            var mappingId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetSectionSubjects),
                new { sectionId }
                //ApiResponse<Guid>.SuccessResponse(mappingId, "Subject mapped successfully")
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
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 400)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> UpdateSubjectMapping(Guid sectionId, Guid mappingId, [FromBody] UpdateSubjectMappingCommand request)
        {
            var command = new UpdateSubjectMappingCommand
            {
                MappingId = mappingId,
                TeacherId = request.TeacherId,
                TeacherName = request.TeacherName,
                WeeklyPeriods = request.WeeklyPeriods,
            };

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Remove subject mapping from section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="mappingId">Mapping ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{mappingId}")]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<Unit>), 404)]
        public async Task<IActionResult> RemoveSubjectMapping(Guid sectionId, Guid mappingId)
        {
            var command = new RemoveSubjectMappingCommand
            {
                MappingId = mappingId,
            };

            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Bulk map multiple subjects to a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="request">Bulk mapping request</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("bulk")]
        //[ProducesResponseType(typeof(ApiResponse<BulkMapResult>), 200)]
        //[ProducesResponseType(typeof(ApiResponse<BulkMapResult>), 400)]
        public async Task<IActionResult> BulkMapSubjects(
            Guid sectionId,
            [FromBody] BulkMapSubjectsCommand request)
        {
            var command = new BulkMapSubjectsCommand
            {
                SectionId = sectionId,
                SubjectMappings = request.SubjectMappings,
            };

            var result = await _mediator.Send(command);
            return Ok();
        }
    }

    ///// <summary>
    ///// Request model for mapping a subject
    ///// </summary>
    //public class MapSubjectRequest
    //{
    //    public Guid SubjectId { get; set; }
    //    public string SubjectName { get; set; }
    //    public string SubjectCode { get; set; }
    //    public Guid TeacherId { get; set; }
    //    public string TeacherName { get; set; }
    //    public int WeeklyPeriods { get; set; }
    //    public bool IsMandatory { get; set; }
    //}

    ///// <summary>
    ///// Request model for updating subject mapping
    ///// </summary>
    //public class UpdateSubjectMappingRequest
    //{
    //    public Guid TeacherId { get; set; }
    //    public string TeacherName { get; set; }
    //    public int WeeklyPeriods { get; set; }
    //}

    ///// <summary>
    ///// Request model for bulk mapping subjects
    ///// </summary>
    //public class BulkMapSubjectsRequest
    //{
    //    public List<SubjectMappingDto> SubjectMappings { get; set; }
    //}
}
