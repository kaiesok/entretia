namespace Entretia.Api.Ports;

/// <summary>
/// Port de hachage de mot de passe. Le reste du code ne sait pas QUEL
/// algorithme est utilise (regle 1 du contrat de dev : le coeur ne
/// connait pas l'exterieur). L'implementation vit dans Adapters/.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produit une empreinte auto-descriptive (algorithme + parametres + sel + hash).</summary>
    string Hash(string password);

    /// <summary>Verifie un mot de passe contre une empreinte stockee, en temps constant.</summary>
    bool Verify(string password, string storedHash);
}
