using Entretia.Api.Features.Health;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Entretia.Api.Tests.Features.Health;

/// <summary>
/// Test unitaire du controleur (sans serveur HTTP ni base de donnees) :
/// rapide, deterministe, sans dependance externe. Les tests d'integration
/// avec WebApplicationFactory arriveront avec le Sprint 1 (auth).
/// </summary>
public sealed class HealthControllerTests
{
    [Fact]
    public void Check_RepondOkAvecLeNomDuService()
    {
        var controller = new HealthController();

        var result = controller.Check();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<HealthResponse>(ok.Value);
        Assert.Equal("ok", body.Status);
        Assert.Equal("entretia-api", body.Service);
    }
}
