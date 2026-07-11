using System.Text;
using Entretia.Api.Adapters;
using Entretia.Api.Common;
using Entretia.Api.Data;
using Entretia.Api.Features.Auth;
using Entretia.Api.Ports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration validee au demarrage (fail fast) ---
builder.Services.AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection(AppSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Default est obligatoire. " +
        "En developpement : verifier appsettings.Development.json. " +
        "En production : fournir la variable d'environnement ConnectionStrings__Default.");
}

// --- Services ---
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Ports -> Adapters (contrat de dev, regle 1 : aucun SDK hors de Adapters/)
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Bouton "Authorize" dans Swagger pour tester les endpoints proteges.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Coller le jeton renvoye par /api/auth/login.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

// --- Authentification JWT ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret.Length >= 32 ? jwtSettings.Secret : new string('0', 32))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

const string WebCorsPolicy = "web";
builder.Services.AddCors(options =>
{
    var webUrl = builder.Configuration[$"{AppSettings.SectionName}:{nameof(AppSettings.WebUrl)}"];
    options.AddPolicy(WebCorsPolicy, policy =>
        policy.WithOrigins(webUrl ?? string.Empty)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// --- Pipeline HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(WebCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Point d'entree expose pour les tests d'integration (WebApplicationFactory).</summary>
public partial class Program;
