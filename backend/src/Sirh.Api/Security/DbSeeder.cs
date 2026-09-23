using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sirh.Domain.Personnel;
using Sirh.Domain.Security;
using Sirh.Domain.Tenancy;
using Sirh.Infrastructure;
using Sirh.Infrastructure.Identity;

namespace Sirh.Api.Security;

/// <summary>
/// Crée, au premier démarrage, une société de démonstration, le rôle « Administrateur »
/// (avec toutes les permissions connues) et un compte administrateur à partir de Seed:AdminEmail
/// / Seed:AdminPassword. Idempotent : ne recrée rien si ces éléments existent déjà.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var dbContext = services.GetRequiredService<SirhDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = await dbContext.Tenants.FirstOrDefaultAsync();
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Société de démonstration",
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();
        }

        var establishment = await dbContext.Establishments.FirstOrDefaultAsync(e => e.TenantId == tenant.Id);
        if (establishment is null)
        {
            establishment = new Establishment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Siège",
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.Establishments.Add(establishment);
            await dbContext.SaveChangesAsync();
        }

        const string adminRoleName = "Administrateur";
        var adminRole = await roleManager.FindByNameAsync(adminRoleName);
        if (adminRole is null)
        {
            adminRole = new ApplicationRole(adminRoleName) { Id = Guid.NewGuid() };
            await roleManager.CreateAsync(adminRole);
        }

        var existingPermissions = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.Permission)
            .ToListAsync();

        foreach (var permission in Permissions.Toutes.Except(existingPermissions))
        {
            dbContext.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, Permission = permission });
        }

        await dbContext.SaveChangesAsync();

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                TenantId = tenant.Id,
                FullName = "Administrateur"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Impossible de créer le compte administrateur initial : " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}
