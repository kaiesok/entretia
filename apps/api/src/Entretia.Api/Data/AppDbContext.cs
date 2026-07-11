using Microsoft.EntityFrameworkCore;

namespace Entretia.Api.Data;

/// <summary>
/// Contexte EF Core — la description de la base de donnees vit dans le code.
/// Tout changement de schema passe par une migration versionnee :
///   dotnet ef migrations add NomDeLaMigration
///   dotnet ef database update
/// On ne modifie JAMAIS la base a la main.
///
/// Sprint 1 ajoutera : User, Consent
/// Sprint 2 ajoutera : Resume
/// Sprint 3 ajoutera : InterviewSession, InterviewTurn
/// Sprint 4 ajoutera : Report
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Les DbSet<> seront ajoutes sprint par sprint.
}
