using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.TimeOff;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/temps/absences")]
public sealed class AbsencesController(AbsenceService absenceService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.LireAbsences)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await absenceService.ListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = "permission:" + Permissions.GererAbsences)]
    public async Task<IActionResult> Request(RequestAbsenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await absenceService.RequestAsync(request, cancellationToken));
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/decider")]
    [Authorize(Policy = "permission:" + Permissions.GererAbsences)]
    public async Task<IActionResult> Decide(Guid id, DecideAbsenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await absenceService.DecideAsync(id, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
