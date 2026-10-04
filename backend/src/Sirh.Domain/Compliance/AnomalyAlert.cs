using Sirh.Domain.Common;

namespace Sirh.Domain.Compliance;

/// <summary>
/// Alerte détectée automatiquement à la clôture d'un mois (contrôle à règles ou statistique —
/// voir Sirh.Application.Compliance.AnomalyDetectionService). Cycle de vie volontairement
/// simple : détectée → confirmée (vrai problème) ou faux positif, décidée par un gestionnaire.
/// </summary>
public sealed class AnomalyAlert : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PayslipId { get; set; }
    public Guid EmployeeId { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    /// <summary>Code de la règle qui a déclenché l'alerte (ex. "NetNegatifOuNul"), stable et consultable dans le code.</summary>
    public string RuleCode { get; set; } = string.Empty;
    public AnomalySeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public AnomalyStatus Status { get; set; } = AnomalyStatus.Detectee;
    public string? DecisionComment { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
}
