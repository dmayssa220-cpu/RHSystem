using Sirh.Domain.Common;

namespace Sirh.Domain.Personnel;

/// <summary>Un établissement (site) d'une société. Modèle minimal, à enrichir à l'usage (matricule fiscal, adresse structurée…).</summary>
public sealed class Establishment : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
