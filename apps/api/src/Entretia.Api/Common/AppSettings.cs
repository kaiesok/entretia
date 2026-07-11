using System.ComponentModel.DataAnnotations;

namespace Entretia.Api.Common;

/// <summary>
/// Configuration applicative validee au demarrage (fail fast) :
/// si une valeur obligatoire manque, le serveur refuse de demarrer
/// avec un message explicite, plutot que d'echouer plus tard.
/// Equivalent .NET du pattern Options + ValidateOnStart.
/// </summary>
public sealed class AppSettings
{
    public const string SectionName = "App";

    [Required(ErrorMessage = "App:WebUrl est obligatoire (origine autorisee pour CORS).")]
    [Url(ErrorMessage = "App:WebUrl doit etre une URL valide.")]
    public string WebUrl { get; init; } = string.Empty;
}
