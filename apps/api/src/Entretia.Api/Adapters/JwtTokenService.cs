using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Entretia.Api.Common;
using Entretia.Api.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Entretia.Api.Adapters;

/// <summary>Emission de jetons JWT signes en HMAC-SHA256.</summary>
public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public AuthToken Create(Guid userId, string email)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
