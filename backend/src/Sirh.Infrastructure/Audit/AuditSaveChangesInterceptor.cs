using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sirh.Application.Security;

namespace Sirh.Infrastructure.Audit;

/// <summary>
/// Ajoute automatiquement une ligne d'audit pour chaque insertion, modification ou
/// suppression (hors table d'audit elle-même), juste avant l'enregistrement en base.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLogEntry
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            context.Set<AuditLogEntry>().Add(new AuditLogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                UserId = currentUser.UserId,
                TenantId = currentUser.TenantId,
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                Action = entry.State switch
                {
                    EntityState.Added => "Ajout",
                    EntityState.Modified => "Modification",
                    EntityState.Deleted => "Suppression",
                    _ => entry.State.ToString()
                },
                ChangesJson = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue))
            });
        }
    }

    private static string GetPrimaryKeyValue(EntityEntry entry) =>
        string.Join(",", entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? "null"));
}
