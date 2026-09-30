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

    /// <summary>Clôture un mois : calcule et enregistre le bulletin de chaque salarié actif. Irréversible (bulletins immuables).</summary>
    [HttpPost("cloturer")]
    [Authorize(Policy = "permission:" + Permissions.GererDeclarations)]
    public async Task<IActionResult> CloseMonth(CloseMonthRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await payrollService.CloseMonthAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Déclaration Trimestrielle des Salaires (CNSS), à partir des bulletins déjà clôturés.</summary>
    [HttpGet("declarations/cnss/{year:int}/{quarter:int}")]
    [Authorize(Policy = "permission:" + Permissions.GererDeclarations)]
    public async Task<IActionResult> CnssDeclaration(int year, int quarter, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await payrollService.GetCnssQuarterlyDeclarationAsync(year, quarter, cancellationToken));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Déclaration mensuelle de retenue à la source (IRPP + CSS), à partir des bulletins déjà clôturés.</summary>
    [HttpGet("declarations/retenue-source/{year:int}/{month:int}")]
    [Authorize(Policy = "permission:" + Permissions.GererDeclarations)]
    public async Task<IActionResult> WithholdingDeclaration(int year, int month, CancellationToken cancellationToken) =>
        Ok(await payrollService.GetWithholdingDeclarationAsync(year, month, cancellationToken));
}
