using System.ComponentModel.DataAnnotations;

namespace Entretia.Api.Features.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required] string Password,
    [Required(ErrorMessage = "Le prenom est obligatoire."), MaxLength(100)] string FirstName,
    [Required(ErrorMessage = "Le nom est obligatoire."), MaxLength(100)] string LastName,
    [RegularExpression(@"^\+?[0-9 .-]{8,20}$", ErrorMessage = "Le numero de telephone n'est pas valide.")]
    string? Phone,
    bool AcceptDataProcessing);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

/// <summary>Reponse commune a l'inscription et a la connexion.</summary>
public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    DateTimeOffset ExpiresAt);

/// <summary>
/// Resultat des operations d'authentification : succes + donnees, ou echec + message.
/// Evite d'utiliser les exceptions pour des cas metier attendus (email deja pris...).
/// </summary>
public sealed record AuthResult(bool Success, string? Error, AuthResponse? Response)
{
    public static AuthResult Ok(AuthResponse response) => new(true, null, response);
    public static AuthResult Fail(string error) => new(false, error, null);
}
