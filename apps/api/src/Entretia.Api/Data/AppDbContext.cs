using Entretia.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entretia.Api.Data;

/// <summary>
/// Contexte EF Core — la description de la base vit dans le code.
/// Tout changement de schema passe par une migration versionnee :
///   dotnet ef migrations add NomDeLaMigration
///   dotnet ef database update
/// On ne modifie JAMAIS la base a la main.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Consent> Consents => Set<Consent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Email).HasMaxLength(320);
            user.Property(u => u.PasswordHash).HasMaxLength(500);
            user.Property(u => u.FirstName).HasMaxLength(100);
            user.Property(u => u.LastName).HasMaxLength(100);
            user.Property(u => u.Phone).HasMaxLength(20);
            user.Property(u => u.Locale).HasMaxLength(10);
        });

        modelBuilder.Entity<Consent>(consent =>
        {
            consent.Property(c => c.Type).HasConversion<string>().HasMaxLength(40);
            consent.HasOne(c => c.User)
                   .WithMany(u => u.Consents)
                   .HasForeignKey(c => c.UserId);
        });
    }
}
