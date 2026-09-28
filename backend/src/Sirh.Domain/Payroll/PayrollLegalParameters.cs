using System.Text.Json;

namespace Sirh.Domain.Payroll;

/// <summary>
/// Référentiel réglementaire de paie (taux CNSS, barème IRPP, CSS, déductions), daté et sourcé.
/// National, pas rattaché à une société : partagé par toutes les sociétés de la plateforme.
/// Plusieurs versions peuvent coexister (une par EffectiveFrom) ; le calcul retient toujours
/// la plus récente dont la date d'effet n'est pas dans le futur.
/// </summary>
public sealed class PayrollLegalParameters
{
    public Guid Id { get; set; }
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>Référence du texte (loi de finances, taux CNSS...) : toujours renseignée, jamais un taux "en dur" sans origine.</summary>
    public string Source { get; set; } = string.Empty;

    public decimal CnssEmployeeRate { get; set; }
    public decimal CnssEmployerRate { get; set; }
    public decimal? CnssCeilingAnnual { get; set; }

    public decimal ProfessionalDeductionRate { get; set; }
    public decimal ProfessionalDeductionCeilingAnnual { get; set; }

    public decimal CssRate { get; set; }

    public decimal FamilyDeductionHeadOfHousehold { get; set; }
    public decimal FamilyDeductionPerChild { get; set; }
    public int FamilyDeductionMaxChildren { get; set; }

    /// <summary>Barème IRPP sérialisé en JSON (liste ordonnée de tranches) — voir GetBrackets/SetBrackets.</summary>
    public string BracketsJson { get; set; } = "[]";

    public IReadOnlyList<LegalBracket> GetBrackets() =>
        JsonSerializer.Deserialize<List<LegalBracket>>(BracketsJson) ?? [];

    public void SetBrackets(IEnumerable<LegalBracket> brackets) =>
        BracketsJson = JsonSerializer.Serialize(brackets);
}
