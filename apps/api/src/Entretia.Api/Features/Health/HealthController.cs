using Microsoft.AspNetCore.Mvc;

namespace Entretia.Api.Features.Health;

/// <summary>
/// Organisation par fonctionnalite (feature folders) : chaque dossier de
/// Features/ contient le controleur, les contrats et bientot les services
/// d'un domaine metier. Sprint 1 ajoutera Features/Auth, Sprint 2
/// Features/Resumes, etc.
/// </summary>
[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Check()
        => Ok(new HealthResponse("ok", "entretia-api", DateTimeOffset.UtcNow));
}
