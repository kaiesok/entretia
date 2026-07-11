namespace Entretia.Api.Features.Health;

/// <summary>
/// Contrat de reponse du endpoint /api/health.
/// Expose dans la documentation OpenAPI : le frontend genere ses types
/// TypeScript a partir de ce contrat (voir README, section "Types partages").
/// </summary>
public sealed record HealthResponse(string Status, string Service, DateTimeOffset Timestamp);
