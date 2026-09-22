using Microsoft.EntityFrameworkCore;
using Serilog;
using Sirh.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "La chaîne de connexion 'ConnectionStrings:Default' est manquante. " +
        "Vérifie la variable d'environnement ConnectionStrings__Default (voir docker-compose.yml).");
}

// Version fixée plutôt que ServerVersion.AutoDetect(...) : évite une connexion à la base
// dès le démarrage de l'application (AutoDetect échouerait si MySQL n'est pas encore prêt).
var serverVersion = new MySqlServerVersion(new Version(8, 4, 0));

builder.Services.AddDbContext<SirhDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

var app = builder.Build();

app.MapControllers();

app.Run();
