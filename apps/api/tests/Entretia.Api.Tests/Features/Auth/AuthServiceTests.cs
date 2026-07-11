using Entretia.Api.Data;
using Entretia.Api.Features.Auth;
using Entretia.Api.Ports;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Entretia.Api.Tests.Features.Auth;

/// <summary>
/// Tests du service d'authentification.
/// - Base : SQLite EN MEMOIRE (vraie base relationnelle, zero installation,
///   detruite a la fin du test) — jamais la base de dev.
/// - Hachage et jetons : FAUX deterministes injectes via les ports
///   (regle absolue du contrat : aucun test ne depend d'un service reel).
/// </summary>
public sealed class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _service = new AuthService(_db, new FakePasswordHasher(), new FakeTokenService());
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static RegisterRequest ValidRegistration(string email = "yasmine@example.com")
        => new(
            email,
            "un mot de passe tres solide",
            FirstName: "Yasmine",
            LastName: "Ben Salah",
            Phone: null,
            AcceptDataProcessing: true);

    // ---------- Inscription ----------

    [Fact]
    public async Task Inscription_Valide_CreeLeCompteEtRetourneUnJeton()
    {
        var result = await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal("yasmine@example.com", result.Response.Email);
        Assert.Equal("Yasmine", result.Response.FirstName);
        Assert.Equal("fake-token", result.Response.AccessToken);
        Assert.Equal(1, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task Inscription_EnregistreLeConsentementRgpd()
    {
        await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        var consent = await _db.Consents.SingleAsync();
        Assert.Equal(Data.Entities.ConsentType.DataProcessing, consent.Type);
        Assert.Null(consent.RevokedAt);
    }

    [Fact]
    public async Task Inscription_SansConsentement_EstRefusee()
    {
        var request = ValidRegistration() with { AcceptDataProcessing = false };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task Inscription_EmailDejaUtilise_EstRefusee()
    {
        await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        var result = await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(1, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task Inscription_EmailAvecMajuscules_EstNormaliseEnMinuscules()
    {
        await _service.RegisterAsync(ValidRegistration("Yasmine@Example.COM"), CancellationToken.None);

        var user = await _db.Users.SingleAsync();
        Assert.Equal("yasmine@example.com", user.Email);
    }

    [Fact]
    public async Task Inscription_PrenomVideOuEspaces_EstRefusee()
    {
        var request = ValidRegistration() with { FirstName = "   " };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task Inscription_PrenomEtNom_SontNettoyesDesEspaces()
    {
        var request = ValidRegistration() with { FirstName = "  Yasmine ", LastName = " Ben Salah  " };

        await _service.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.SingleAsync();
        Assert.Equal("Yasmine", user.FirstName);
        Assert.Equal("Ben Salah", user.LastName);
    }

    [Fact]
    public async Task Inscription_TelephoneAbsent_EstAcceptee()
    {
        var request = ValidRegistration() with { Phone = "   " };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        var user = await _db.Users.SingleAsync();
        Assert.Null(user.Phone); // chaine vide normalisee en null : pas de fausse donnee
    }

    [Fact]
    public async Task Inscription_TelephoneFourni_EstEnregistre()
    {
        var request = ValidRegistration() with { Phone = "+216 20 123 456" };

        await _service.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.SingleAsync();
        Assert.Equal("+216 20 123 456", user.Phone);
    }

    [Fact]
    public async Task Inscription_MotDePasseTropCourt_EstRefusee()
    {
        var request = ValidRegistration() with { Password = "court" };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Inscription_LeMotDePasseNestJamaisStockeEnClair()
    {
        var request = ValidRegistration();

        await _service.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.SingleAsync();
        Assert.DoesNotContain("un mot de passe tres solide", user.PasswordHash);
    }

    // ---------- Connexion ----------

    [Fact]
    public async Task Connexion_AvecLesBonsIdentifiants_Reussit()
    {
        await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        var result = await _service.LoginAsync(
            new LoginRequest("yasmine@example.com", "un mot de passe tres solide"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("fake-token", result.Response!.AccessToken);
        Assert.Equal("Yasmine", result.Response.FirstName);
    }

    [Fact]
    public async Task Connexion_MauvaisMotDePasse_EchoueAvecLeMessageGenerique()
    {
        await _service.RegisterAsync(ValidRegistration(), CancellationToken.None);

        var result = await _service.LoginAsync(
            new LoginRequest("yasmine@example.com", "mauvais mot de passe !"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(AuthService.InvalidCredentialsMessage, result.Error);
    }

    [Fact]
    public async Task Connexion_EmailInconnu_EchoueAvecLeMEMEMessage()
    {
        // Meme message que "mauvais mot de passe" : impossible de deviner
        // quels emails ont un compte (protection contre l'enumeration).
        var result = await _service.LoginAsync(
            new LoginRequest("inconnu@example.com", "peu importe le mdp"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(AuthService.InvalidCredentialsMessage, result.Error);
    }

    // ---------- Faux deterministes (aucun service reel dans les tests) ----------

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"FAKEHASH::{password.Length}::{Reverse(password)}";
        public bool Verify(string password, string storedHash) => storedHash == Hash(password);
        private static string Reverse(string s) => new(s.Reverse().ToArray());
    }

    private sealed class FakeTokenService : IJwtTokenService
    {
        public AuthToken Create(Guid userId, string email)
            => new("fake-token", DateTimeOffset.UtcNow.AddHours(1));
    }
}
