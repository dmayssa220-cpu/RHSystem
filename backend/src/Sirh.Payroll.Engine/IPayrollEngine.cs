namespace Sirh.Payroll.Engine;

/// <summary>
/// Point d'entrée du moteur de paie. Implémenté à l'étape « moteur de paie tunisien » :
/// (contexte salarié + variables du mois + référentiel réglementaire daté) → bulletin + trace de calcul.
/// </summary>
public interface IPayrollEngine
{
    PayrollCalculationResult Calculate(PayrollCalculationRequest request);
}

/// <summary>Entrée du calcul. Complétée au fil du développement du moteur.</summary>
public sealed record PayrollCalculationRequest;

/// <summary>Résultat du calcul (bulletin + trace). Complété au fil du développement du moteur.</summary>
public sealed record PayrollCalculationResult;
