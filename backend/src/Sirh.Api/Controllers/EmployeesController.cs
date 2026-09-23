using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Personnel;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/personnel/salaries")]
public sealed class EmployeesController(EmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.LirePersonnel)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await employeeService.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "permission:" + Permissions.LirePersonnel)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetAsync(id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = "permission:" + Permissions.GererPersonnel)]
    public async Task<IActionResult> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await employeeService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "permission:" + Permissions.GererPersonnel)]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await employeeService.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
