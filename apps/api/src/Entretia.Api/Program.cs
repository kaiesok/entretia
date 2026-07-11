using Entretia.Api.Common;
using Entretia.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration validee au demarrage (fail fast) ---
builder.Services.AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection(AppSettings.SectionName))
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    // Documentation interactive : http://localhost:3001/swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(WebCorsPolicy);
app.MapControllers();

app.Run();

/// <summary>Point d'entree expose pour les tests d'integration (WebApplicationFactory).</summary>
public partial class Program;
