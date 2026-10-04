namespace Sirh.Payroll.Engine;

public sealed record PayrollBracket(decimal? UpperBoundAnnual, decimal Rate);

/// <summary>
/// Paramètres réglementaires nécessaires au calcul. Le moteur ne va jamais les chercher
/// lui-même (pas de base de données ici) : ils lui sont toujours fournis en entrée.
/// </summary>
public sealed record TunisianPayrollParameters(
    DateOnly EffectiveFrom,
    string Source,
    decimal CnssEmployeeRate,
    decimal CnssEmployerRate,
    decimal? CnssCeilingAnnual,
    decimal ProfessionalDeductionRate,
    decimal ProfessionalDeductionCeilingAnnual,
    decimal CssRate,
    decimal FamilyDeductionHeadOfHousehold,
    decimal FamilyDeductionPerChild,
    int FamilyDeductionMaxChildren,
    IReadOnlyList<PayrollBracket> Brackets);

/// <summary>Un élément variable du mois : prime, heures supplémentaires (déjà converties en montant), ou autre ajustement (peut être négatif).</summary>
public sealed record PayrollVariableInput(string Label, decimal Amount);

public sealed record PayrollCalculationRequest(
    decimal BaseSalaryMonthly,
    int UnpaidLeaveDays,
    IReadOnlyList<PayrollVariableInput> Variables,
    bool IsHeadOfHousehold,
    int DependentChildren,
    TunisianPayrollParameters Parameters);

/// <summary>Une ligne de la trace de calcul : ce que voit un utilisateur qui demande "pourquoi ce montant ?".</summary>
public sealed record PayrollTraceLine(string Label, decimal Amount, string? Detail = null);

public sealed record PayrollCalculationResult(
    decimal GrossMonthlySalary,
    decimal CnssEmployeeMonthly,
    decimal CnssEmployerMonthly,
    decimal TaxableAnnual,
    decimal IrppAnnual,
    decimal IrppMonthly,
    decimal CssMonthly,
    decimal NetMonthly,
    IReadOnlyList<PayrollTraceLine> Trace);

/// <summary>
/// Point d'entrée du moteur de paie : fonction pure, (entrées + paramètres légaux) → résultat + trace.
/// Mêmes entrées et mêmes paramètres légaux = toujours le même résultat, ce qui permet de rejouer un
/// calcul à l'identique et d'expliquer chaque montant du bulletin.
/// </summary>
public interface IPayrollEngine
{
    PayrollCalculationResult Calculate(PayrollCalculationRequest request);
}
