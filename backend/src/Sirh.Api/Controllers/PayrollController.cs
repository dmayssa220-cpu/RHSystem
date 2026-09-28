using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Payroll;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/paie")]
public sealed class PayrollController(PayrollService payrollService) : ControllerBase
{
    /// <summary>Calcule un bulletin sans l'enregistrer : sert à explorer/expliquer un montant avant clôture.</summary>
    [HttpPost("simuler")]
    [Authorize(Policy = "permission:" + Permissions.CalculerPaie)]
    public async Task<IActionResult> Preview(PayrollPreviewRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await payrollService.PreviewAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
