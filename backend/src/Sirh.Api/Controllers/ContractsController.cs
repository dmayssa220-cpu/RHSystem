using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Personnel;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/personnel/salaries/{employeeId:guid}/contrats")]
public sealed class ContractsController(ContractService contractService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.LirePersonnel)]
    public async Task<IActionResult> List(Guid employeeId, CancellationToken cancellationToken) =>
        Ok(await contractService.ListForEmployeeAsync(employeeId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "permission:" + Permissions.GererPersonnel)]
    public async Task<IActionResult> Create(Guid employeeId, CreateContractRequest request, CancellationToken cancellationToken)
    {
        if (employeeId != request.EmployeeId)
        {
            return BadRequest(new { message = "L'identifiant du salarié ne correspond pas à l'URL." });
        }

        try
        {
            var created = await contractService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), new { employeeId }, created);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
