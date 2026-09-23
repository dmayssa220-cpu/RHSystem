using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sirh.Api.Controllers;

/// <summary>Vérification simple que l'API répond. Volontairement anonyme (utile pour un futur contrôle Docker).</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
