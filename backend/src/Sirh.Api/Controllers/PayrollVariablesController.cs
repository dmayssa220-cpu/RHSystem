using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Payroll;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/paie/variables")]
public sealed class PayrollVariablesController(PayrollVariableService variableService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.CalculerPaie)]
    public async Task<IActionResult> List([FromQuery] Guid employeeId, [FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken) =>
        Ok(await variableService.ListAsync(employeeId, year, month, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "permission:" + Permissions.GererVariablesPaie)]
    public async Task<IActionResult> Create(CreatePayrollVariableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await variableService.CreateAsync(request, cancellationToken));
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "permission:" + Permissions.GererVariablesPaie)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await variableService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
