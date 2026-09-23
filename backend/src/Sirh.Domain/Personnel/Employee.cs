using Sirh.Domain.Common;

namespace Sirh.Domain.Personnel;

/// <summary>
/// Dossier salarié. Modèle de départ, volontairement resserré : documents, historique
/// détaillé et champs conventionnels viendront s'y ajouter à l'usage.
/// </summary>
public sealed class Employee : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EstablishmentId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? JobPositionId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }

    /// <summary>Numéro de carte d'identité nationale. À chiffrer avant toute mise en production (voir README, section Sécurité).</summary>
    public string NationalId { get; set; } = string.Empty;

    public string? PersonalEmail { get; set; }
    public string? PersonalPhone { get; set; }

    public DateOnly HireDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Actif;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
