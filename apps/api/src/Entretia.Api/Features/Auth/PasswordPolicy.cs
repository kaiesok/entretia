namespace Entretia.Api.Features.Auth;

/// <summary>
/// Politique de mot de passe — logique PURE (aucune E/S), developpee en TDD
/// (voir tests/Features/Auth/PasswordPolicyTests.cs, ecrits AVANT ce code).
///
/// Choix aligne sur les recommandations NIST/OWASP modernes :
/// la LONGUEUR prime, pas les regles de composition ("1 majuscule,
/// 1 chiffre...") qui produisent des mots de passe previsibles.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 12;
    public const int MaxLength = 100;

    public static IReadOnlyList<string> Validate(string? password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Le mot de passe est obligatoire.");
            return errors;
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Le mot de passe doit contenir au moins {MinLength} caracteres.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Le mot de passe ne peut pas depasser {MaxLength} caracteres.");
        }

        return errors;
    }
}
