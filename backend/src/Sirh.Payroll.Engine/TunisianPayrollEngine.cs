namespace Sirh.Payroll.Engine;

/// <summary>
/// Moteur de calcul de la paie tunisienne (régime général, secteur privé non agricole,
/// retenue à la source mensuelle par projection annuelle).
///
/// AVERTISSEMENT : les taux et barèmes par défaut (voir le seed dans Sirh.Api.Security.DbSeeder)
/// sont ceux publiquement disponibles en 2026 (CNSS RSNA, barème IRPP à 8 tranches de la loi de
/// finances 2025, CSS à 0,5 %). Ils n'ont pas été validés par un expert-comptable ou un juriste :
/// à vérifier avant tout calcul de paie réel (voir README, section Sécurité).
/// </summary>
public sealed class TunisianPayrollEngine : IPayrollEngine
{
    /// <summary>Convention utilisée pour proratiser une absence sans solde : un mois "forfaitaire" de 30 jours.</summary>
    private const int DaysPerMonth = 30;

    public PayrollCalculationResult Calculate(PayrollCalculationRequest request)
    {
        var p = request.Parameters;
        var trace = new List<PayrollTraceLine>();

        var baseSalary = Round(request.BaseSalaryMonthly);
        trace.Add(new PayrollTraceLine("Salaire de base du contrat", baseSalary));

        var unpaidDeduction = 0m;
        if (request.UnpaidLeaveDays > 0)
        {
            var dailyRate = Round(baseSalary / DaysPerMonth);
            unpaidDeduction = Round(dailyRate * request.UnpaidLeaveDays);
            trace.Add(new PayrollTraceLine(
                $"Retenue pour absence sans solde ({request.UnpaidLeaveDays} jour(s))", -unpaidDeduction,
                $"{dailyRate:0.000} DT/jour (salaire de base ÷ {DaysPerMonth})"));
        }

        foreach (var variable in request.Variables)
        {
            trace.Add(new PayrollTraceLine(variable.Label, Round(variable.Amount)));
        }

        var variablesTotal = Round(request.Variables.Sum(v => v.Amount));
        var gross = Round(baseSalary - unpaidDeduction + variablesTotal);
        trace.Add(new PayrollTraceLine("Salaire brut mensuel", gross));

        var cnssCeilingMonthly = p.CnssCeilingAnnual.HasValue ? p.CnssCeilingAnnual.Value / 12m : (decimal?)null;
        var cnssBase = cnssCeilingMonthly.HasValue ? Math.Min(gross, cnssCeilingMonthly.Value) : gross;
        var cnssEmployee = Round(cnssBase * p.CnssEmployeeRate);
        trace.Add(new PayrollTraceLine("Cotisation CNSS salariale", cnssEmployee, $"{cnssBase:0.000} DT × {p.CnssEmployeeRate:0.00%} — {p.Source}"));

        var cnssEmployer = Round(cnssBase * p.CnssEmployerRate);
        trace.Add(new PayrollTraceLine("Cotisation CNSS patronale (informative, ne réduit pas le net)", cnssEmployer, $"{cnssBase:0.000} DT × {p.CnssEmployerRate:0.00%}"));

        var netBeforeTaxMonthly = Round(gross - cnssEmployee);
        var netBeforeTaxAnnual = Round(netBeforeTaxMonthly * 12);
        trace.Add(new PayrollTraceLine("Revenu net annuel avant impôt (projection × 12)", netBeforeTaxAnnual));

        var professionalDeduction = Round(Math.Min(netBeforeTaxAnnual * p.ProfessionalDeductionRate, p.ProfessionalDeductionCeilingAnnual));
        trace.Add(new PayrollTraceLine("Déduction forfaitaire pour frais professionnels", professionalDeduction, $"{p.ProfessionalDeductionRate:0%}, plafonnée à {p.ProfessionalDeductionCeilingAnnual:0.000} DT/an"));

        var childrenCounted = Math.Min(Math.Max(request.DependentChildren, 0), p.FamilyDeductionMaxChildren);
        var familyDeduction = Round((request.IsHeadOfHousehold ? p.FamilyDeductionHeadOfHousehold : 0m) + childrenCounted * p.FamilyDeductionPerChild);
        trace.Add(new PayrollTraceLine("Déductions pour charges de famille", familyDeduction, $"Chef de famille : {(request.IsHeadOfHousehold ? "oui" : "non")} ; enfants pris en compte : {childrenCounted}/{p.FamilyDeductionMaxChildren}"));

        var taxableAnnual = Math.Max(0m, Round(netBeforeTaxAnnual - professionalDeduction - familyDeduction));
        trace.Add(new PayrollTraceLine("Revenu net imposable annuel", taxableAnnual));

        var irppAnnual = Round(ApplyBrackets(taxableAnnual, p.Brackets));
        trace.Add(new PayrollTraceLine("IRPP annuel (barème progressif)", irppAnnual, p.Source));

        var cssAnnual = Round(taxableAnnual * p.CssRate);
        trace.Add(new PayrollTraceLine("Contribution sociale de solidarité (CSS) annuelle", cssAnnual, $"{taxableAnnual:0.000} DT × {p.CssRate:0.00%}"));

        var irppMonthly = Round(irppAnnual / 12m);
        var cssMonthly = Round(cssAnnual / 12m);
        trace.Add(new PayrollTraceLine("Retenue IRPP mensuelle", irppMonthly));
        trace.Add(new PayrollTraceLine("Retenue CSS mensuelle", cssMonthly));

        var netMonthly = Round(gross - cnssEmployee - irppMonthly - cssMonthly);
        trace.Add(new PayrollTraceLine("Salaire net à payer", netMonthly));

        return new PayrollCalculationResult(gross, cnssEmployee, cnssEmployer, taxableAnnual, irppAnnual, irppMonthly, cssMonthly, netMonthly, trace);
    }

    /// <summary>Barème progressif par tranches : chaque tranche de revenu n'est imposée qu'à son propre taux.</summary>
    private static decimal ApplyBrackets(decimal taxableAnnual, IReadOnlyList<PayrollBracket> brackets)
    {
        var tax = 0m;
        var previousUpperBound = 0m;

        foreach (var bracket in brackets)
        {
            if (taxableAnnual <= previousUpperBound)
            {
                break;
            }

            var upper = bracket.UpperBoundAnnual ?? decimal.MaxValue;
            var sliceTop = Math.Min(taxableAnnual, upper);
            var sliceWidth = sliceTop - previousUpperBound;

            if (sliceWidth > 0)
            {
                tax += sliceWidth * bracket.Rate;
            }

            previousUpperBound = upper;
        }

        return tax;
    }

    private static decimal Round(decimal value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);
}
