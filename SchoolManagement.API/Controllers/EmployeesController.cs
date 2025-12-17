using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Employees.Commands;
using SchoolManagement.Application.Employees.Queries;
using SchoolManagement.Domain.Common;

[ApiController]
[Route("api/hr/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ---------------- CREATE ----------------
    [HttpPost]
    [Authorize(Roles = "Admin,Principal,HRManager")]
    public async Task<ActionResult<Result<Guid>>> CreateEmployee(CreateEmployeeCommand command)
    {
        var result = await _mediator.Send(command);   // Result<Guid>

        if (!result.Status)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetEmployee),
            new { id = result.Message },
            result);
    }

    // ---------------- GET BY ID ----------------
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Principal,HRManager,DepartmentHead")]
    public async Task<ActionResult<Result<EmployeeDto>>> GetEmployee(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery { Id = id }); // Result<EmployeeDto>

        if (!result.Status || result.Data == null)
            return NotFound(result);

        return Ok(result);
    }

    // ---------------- GET BY DEPARTMENT ----------------
    [HttpGet("department/{departmentId}")]
    [Authorize(Roles = "Admin,Principal,HRManager,DepartmentHead")]
    public async Task<ActionResult<Result<IEnumerable<EmployeeDto>>>> GetEmployeesByDepartment(Guid departmentId)
    {
        var result = await _mediator.Send(new GetEmployeesByDepartmentQuery
        {
            DepartmentId = departmentId
        });

        return Ok(result);
    }

    // ---------------- UPDATE ----------------
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Principal,HRManager")]
    public async Task<ActionResult<Result<EmployeeDto>>> UpdateEmployee(Guid id, UpdateEmployeeCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);  // Result<EmployeeDto>

        if (!result.Status)
            return BadRequest(result);

        return Ok(result);
    }

    // ---------------- ONBOARD ----------------
    [HttpPost("{id}/onboard")]
    [Authorize(Roles = "Admin,Principal,HRManager")]
    public async Task<ActionResult<Result<bool>>> OnboardEmployee(Guid id, OnboardEmployeeCommand command)
    {
        command.EmployeeId = id;
        var result = await _mediator.Send(command);   // Result<bool>

        if (!result.Status)
            return BadRequest(result);

        return Ok(result);
    }
}
