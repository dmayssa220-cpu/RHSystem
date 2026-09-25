using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirh.Application.Personnel;
using Sirh.Domain.Security;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/personnel/etablissements")]
public sealed class EstablishmentsController(EstablishmentService establishmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "permission:" + Permissions.LirePersonnel)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await establishmentService.ListAsync(cancellationToken));
}
