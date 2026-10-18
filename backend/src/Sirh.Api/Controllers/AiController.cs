using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Ai;
using Sirh.Application.Payroll;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/ia")]
public sealed class AiController(
    PayrollReadinessAgent readinessAgent,
    PayrollExplanationAgent explanationAgent,
    HrDocumentService hrDocumentService,
    PayrollService payrollService,
    ILlmClient llmClient) : ControllerBase
{
    /// <summary>Indique si un fournisseur d'IA est configuré, pour que l'interface sache si une reformulation est possible.</summary>
    [HttpGet("statut")]
    [Authorize]
    public IActionResult Status() => Ok(new { configured = llmClient.IsConfigured });

    /// <summary>Agent de préparation de paie : vérifie la complétude des données avant de clôturer un mois.</summary>
    [HttpGet("preparation-paie/{year:int}/{month:int}")]
    [Authorize(Policy = "permission:" + Permissions.GererDeclarations)]
    public async Task<IActionResult> CheckReadiness(int year, int month, CancellationToken cancellationToken) =>
        Ok(await readinessAgent.CheckAsync(year, month, cancellationToken));

    /// <summary>Reformule, si un fournisseur est configuré, l'explication d'une comparaison de paie déjà calculée.</summary>
    [HttpGet("explication-paie/{employeeId:guid}")]
    [Authorize(Policy = "permission:" + Permissions.CalculerPaie)]
    public async Task<IActionResult> ExplainComparison(
        Guid employeeId, [FromQuery] int yearBefore, [FromQuery] int monthBefore, [FromQuery] int yearAfter, [FromQuery] int monthAfter, CancellationToken cancellationToken)
    {
        try
        {
            var comparison = await payrollService.CompareAsync(employeeId, yearBefore, monthBefore, yearAfter, monthAfter, cancellationToken);
            var (text, reformulated) = await explanationAgent.ExplainAsync(comparison.Narrative, cancellationToken);
            return Ok(new { text, reformulated });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Génère une attestation de travail à partir du dossier salarié.</summary>
    [HttpGet("documents/attestation-travail/{employeeId:guid}")]
    [Authorize(Policy = "permission:" + Permissions.GererPersonnel)]
    public async Task<IActionResult> WorkCertificate(Guid employeeId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await hrDocumentService.GenerateWorkCertificateAsync(employeeId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
