using Microsoft.AspNetCore.Mvc;

namespace Sirh.Api.Controllers;

/// <summary>Vérification simple que l'API répond (utilisée par les futurs contrôles de santé Docker).</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
