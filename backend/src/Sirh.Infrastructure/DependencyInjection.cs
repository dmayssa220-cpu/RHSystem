using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sirh.Application.Persistence;
using Sirh.Infrastructure.Audit;

namespace Sirh.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Enregistre le contexte de base de données et ses dépendances (interception d'audit).
    /// La chaîne de connexion est passée en paramètre plutôt que lue ici depuis IConfiguration,
    /// pour que cette couche n'ait besoin d'aucune dépendance de configuration.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<SirhDbContext>((serviceProvider, options) =>
        {
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0)));
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<SirhDbContext>());

        return services;
    }
}
