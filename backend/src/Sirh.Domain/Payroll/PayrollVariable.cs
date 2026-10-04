using Sirh.Domain.Common;

namespace Sirh.Domain.Payroll;

/// <summary>
/// Élément variable de paie pour un salarié, un mois donné (prime, heures supplémentaires déjà
/// converties en montant, autre ajustement). Ajouté au salaire de base avant calcul des
/// cotisations et de l'impôt — voir Sirh.Payroll.Engine.
/// </summary>
public sealed class PayrollVariable : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }

    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    public PayrollVariableType Type { get; set; }
    public string Label { get; set; } = string.Empty;

    /// <summary>Montant en dinars ; positif (prime, heures sup) ou négatif (retenue exceptionnelle).</summary>
    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
