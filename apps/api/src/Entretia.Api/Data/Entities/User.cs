namespace Entretia.Api.Data.Entities;

/// <summary>
/// Compte utilisateur. Le mot de passe n'est JAMAIS stocke : seule son
/// empreinte argon2id l'est (voir Adapters/Argon2PasswordHasher).
/// Minimisation RGPD : on ne collecte que le necessaire au service
/// (identite pour personnaliser les entretiens, telephone optionnel).
/// </summary>
public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Toujours normalise en minuscules avant stockage (unicite fiable).</summary>
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>Optionnel — jamais exige pour utiliser le service.</summary>
    public string? Phone { get; set; }

    public string Locale { get; set; } = "fr";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Consent> Consents { get; set; } = [];
}
