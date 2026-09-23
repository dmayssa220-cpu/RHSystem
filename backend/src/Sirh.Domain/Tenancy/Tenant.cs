namespace Sirh.Domain.Tenancy;

/// <summary>
/// Une société cliente (locataire). Modèle volontairement minimal : le détail
/// (établissements, SIRET/matricule fiscal…) arrive avec le module Personnel.
/// </summary>
public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
