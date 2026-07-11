using System.ComponentModel.DataAnnotations;

namespace Entretia.Api.Common;

/// <summary>
/// Configuration JWT, validee au demarrage (fail fast).
/// Le Secret ne vit ni dans appsettings.json ni dans Git :
///   dev  -> dotnet user-secrets set "Jwt:Secret" "..."
///   prod -> variable d'environnement Jwt__Secret
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "Jwt:Secret est obligatoire. Dev : dotnet user-secrets set \"Jwt:Secret\" \"<32+ caracteres>\"")]
    [MinLength(32, ErrorMessage = "Jwt:Secret doit faire au moins 32 caracteres.")]
    public string Secret { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "entretia-api";

    [Range(5, 1440)]
    public int ExpiresMinutes { get; init; } = 60;
}
