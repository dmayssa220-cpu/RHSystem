using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Application.Security;
using Sirh.Domain.Personnel;
using Sirh.Domain.Tenancy;
using Sirh.Infrastructure.Audit;
using Sirh.Infrastructure.Identity;

namespace Sirh.Infrastructure;

/// <summary>
/// Contexte de base de données de l'application. Étend IdentityDbContext pour obtenir
/// gratuitement les tables utilisateurs/rôles standard d'ASP.NET Core Identity, et
/// implémente IAppDbContext pour que la couche application y accède sans dépendre d'EF Core.
/// </summary>
public sealed class SirhDbContext(DbContextOptions<SirhDbContext> options, ICurrentUserService currentUser)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IAppDbContext
{
    // Capturé une fois à la création du contexte (qui est "scoped" : une instance par requête
    // HTTP), donc réévalué à chaque requête. C'est la base du filtre de cloisonnement ci-dessous.
    private readonly Guid? _tenantId = currentUser.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    public DbSet<Establishment> Establishments => Set<Establishment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Contract> Contracts => Set<Contract>();

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

        builder.ApplyConfigurationsFromAssembly(typeof(SirhDbContext).Assembly);

        // Cloisonnement par société : chaque requête sur ces tables ne voit que les données
        // de la société de l'utilisateur courant. Un utilisateur sans société (administrateur
        // de plateforme, _tenantId == null) voit tout — il n'y en a pas encore aujourd'hui.
        builder.Entity<Establishment>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        builder.Entity<Department>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        builder.Entity<JobPosition>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        builder.Entity<Employee>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        builder.Entity<Contract>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
    }
}
