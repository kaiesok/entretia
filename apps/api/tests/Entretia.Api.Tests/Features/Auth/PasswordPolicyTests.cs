using Entretia.Api.Features.Auth;
using Xunit;

namespace Entretia.Api.Tests.Features.Auth;

/// <summary>
/// Tests TDD de la politique de mot de passe — ecrits AVANT PasswordPolicy.cs.
/// Chaque test decrit une REGLE METIER ; le code n'existe que pour les faire passer.
/// </summary>
public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MotDePasseVide_EstRefuse(string? password)
    {
        var errors = PasswordPolicy.Validate(password);

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void MoinsDe12Caracteres_EstRefuse()
    {
        var errors = PasswordPolicy.Validate("abcdefghijk"); // 11 caracteres

        Assert.Contains(errors, e => e.Contains("12"));
    }

    [Fact]
    public void Exactement12Caracteres_EstAccepte()
    {
        var errors = PasswordPolicy.Validate("abcdefghijkl"); // 12 caracteres

        Assert.Empty(errors);
    }

    [Fact]
    public void PhraseDePasseAvecEspaces_EstAcceptee()
    {
        // Pas de regles de composition : une phrase longue est un excellent mot de passe.
        var errors = PasswordPolicy.Validate("le jasmin fleurit a bordj el amri");

        Assert.Empty(errors);
    }

    [Fact]
    public void PlusDe100Caracteres_EstRefuse()
    {
        var errors = PasswordPolicy.Validate(new string('a', 101));

        Assert.NotEmpty(errors);
    }
}
