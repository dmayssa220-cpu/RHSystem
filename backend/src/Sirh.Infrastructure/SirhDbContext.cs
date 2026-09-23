using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sirh.Domain.Tenancy;
using Sirh.Infrastructure.Audit;
using Sirh.Infrastructure.Identity;

namespace Sirh.Infrastructure;

/// <summary>
/// Contexte de base de données de l'application. Étend IdentityDbContext pour obtenir
/// gratuitement les tables utilisateurs/rôles standard d'ASP.NET Core Identity.
/// </summary>
public sealed class SirhDbContext(DbContextOptions<SirhDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.Permission });
            entity.Property(rp => rp.Permission).HasMaxLength(128);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.Property(t => t.TokenHash).HasMaxLength(64);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(200);
        });

        // Le filtre global de cloisonnement par société (basé sur Sirh.Domain.Common.IHasTenant)
        // sera ajouté ici à l'étape « modèle de données et module Personnel », quand les premières
        // entités métier concernées par le multi-société existeront.
    }
}
