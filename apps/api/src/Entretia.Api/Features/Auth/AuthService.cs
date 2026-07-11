using Entretia.Api.Data;
using Entretia.Api.Data.Entities;
using Entretia.Api.Ports;
using Microsoft.EntityFrameworkCore;

namespace Entretia.Api.Features.Auth;

/// <summary>
/// Logique d'inscription et de connexion. Depend uniquement du DbContext
/// et des PORTS (IPasswordHasher, IJwtTokenService) — jamais d'un SDK :
/// c'est ce qui permet de la tester sans argon2 reel ni vraie cle JWT.
/// </summary>
public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokens)
{
    public const string InvalidCredentialsMessage = "Email ou mot de passe incorrect.";

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        // Les [Required] laissent passer des espaces : on re-verifie apres Trim.
        if (firstName.Length == 0 || lastName.Length == 0)
        {
            return AuthResult.Fail("Le prenom et le nom sont obligatoires.");
        }

        var passwordErrors = PasswordPolicy.Validate(request.Password);
        if (passwordErrors.Count > 0)
        {
            return AuthResult.Fail(string.Join(" ", passwordErrors));
        }

        if (!request.AcceptDataProcessing)
        {
            return AuthResult.Fail("Le consentement au traitement des donnees est requis pour creer un compte.");
        }

        var emailTaken = await db.Users.AnyAsync(u => u.Email == email, ct);
        if (emailTaken)
        {
            return AuthResult.Fail("Un compte existe deja avec cet email.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Consents = [new Consent { Type = ConsentType.DataProcessing }],
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = tokens.Create(user.Id, user.Email);
        return AuthResult.Ok(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, token.AccessToken, token.ExpiresAt));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

        // Message IDENTIQUE que l'email existe ou non : on ne permet pas
        // de deviner quels emails ont un compte (enumeration d'utilisateurs).
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return AuthResult.Fail(InvalidCredentialsMessage);
        }

        var token = tokens.Create(user.Id, user.Email);
        return AuthResult.Ok(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName, token.AccessToken, token.ExpiresAt));
    }
}
