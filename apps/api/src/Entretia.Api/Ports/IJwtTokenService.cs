namespace Entretia.Api.Ports;

/// <summary>Jeton d'acces emis apres authentification.</summary>
public sealed record AuthToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Port d'emission de jetons. Derriere une interface pour pouvoir
/// fournir un faux deterministe dans les tests.
/// </summary>
public interface IJwtTokenService
{
    AuthToken Create(Guid userId, string email);
}
