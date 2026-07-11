namespace Entretia.Api.Data.Entities;

public enum ConsentType
{
    /// <summary>Traitement des donnees personnelles (obligatoire a l'inscription).</summary>
    DataProcessing,

    /// <summary>Captation audio pendant les entretiens (demande au Sprint 5).</summary>
    AudioCapture,

    /// <summary>Captation video pendant les entretiens (demande au Sprint 5).</summary>
    VideoCapture,
}

/// <summary>
/// Trace de consentement RGPD : qui a accepte quoi, quand, et si c'est revoque.
/// On ne SUPPRIME jamais une ligne : une revocation renseigne RevokedAt
/// (piste d'audit exigee par le RGPD).
/// </summary>
public sealed class Consent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ConsentType Type { get; set; }

    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RevokedAt { get; set; }
}
