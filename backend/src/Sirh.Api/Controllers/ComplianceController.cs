using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Compliance;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/conformite/anomalies")]
public sealed class ComplianceController(AnomalyDetectionService anomalyDetection) : ControllerBase
{
    /// <summary>Liste les alertes, éventuellement filtrées par statut (Detectee, Confirmee, FauxPositif).</summary>
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.LireAnomalies)]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await anomalyDetection.ListAsync(status, cancellationToken));
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Statut invalide : Detectee, Confirmee ou FauxPositif." });
        }
    }

    [HttpPost("{id:guid}/decider")]
    [Authorize(Policy = "permission:" + Permissions.GererAnomalies)]
    public async Task<IActionResult> Decide(Guid id, DecideAnomalyRequest request, CancellationToken cancellationToken)
    {
        var result = await anomalyDetection.DecideAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
