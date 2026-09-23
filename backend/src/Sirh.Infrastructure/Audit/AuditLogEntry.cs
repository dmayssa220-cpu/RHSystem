namespace Sirh.Infrastructure.Audit;

/// <summary>
/// Une ligne du journal d'audit : qui a fait quoi, quand. Le compte MySQL "app" n'a que
/// SELECT/INSERT sur cette table (voir docker/mysql/init) : une ligne ne peut pas être
/// modifiée ni supprimée après coup, même par l'application elle-même.
/// </summary>
public sealed class AuditLogEntry
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ChangesJson { get; set; }
}
