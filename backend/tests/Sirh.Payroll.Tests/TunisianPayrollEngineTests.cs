using Sirh.Payroll.Engine;

namespace Sirh.Payroll.Tests;

public class TunisianPayrollEngineTests
{
    // Barème IRPP 2025 (8 tranches), CNSS RSNA et CSS 2026 tels que publiquement disponibles
    // à l'écriture de ce test — mêmes valeurs que le jeu de données amorcé au démarrage
    // (voir Sirh.Api.Security.DbSeeder). À recaler si un expert-comptable corrige ces chiffres.
    private static readonly TunisianPayrollParameters Parameters2026 = new(
        EffectiveFrom: new DateOnly(2026, 1, 1),
        Source: "Loi de finances 2025 — barème IRPP 8 tranches, CNSS RSNA, CSS 2026 (à valider par un expert)",
        CnssEmployeeRate: 0.0968m,
        CnssEmployerRate: 0.1707m,
        CnssCeilingAnnual: null,
        ProfessionalDeductionRate: 0.10m,
        ProfessionalDeductionCeilingAnnual: 2000m,
        CssRate: 0.005m,
        FamilyDeductionHeadOfHousehold: 300m,
        FamilyDeductionPerChild: 100m,
        FamilyDeductionMaxChildren: 4,
        Brackets:
        [
            new PayrollBracket(5000m, 0m),
            new PayrollBracket(10000m, 0.15m),
            new PayrollBracket(20000m, 0.25m),
            new PayrollBracket(30000m, 0.30m),
            new PayrollBracket(40000m, 0.33m),
            new PayrollBracket(50000m, 0.36m),
            new PayrollBracket(70000m, 0.38m),
            new PayrollBracket(null, 0.40m)
        ]);

    private static readonly IPayrollEngine Engine = new TunisianPayrollEngine();
    private static readonly IReadOnlyList<PayrollVariableInput> NoVariables = [];

    [Fact]
    public void Bareme_progressif_24000_dinars_imposables_donne_4450_dinars_d_impot()
    {
        // CNSS, déduction professionnelle et déductions familiales désactivées, pour isoler
        // le barème : un brut de 2 000 DT/mois donne un revenu imposable annuel de 24 000 DT
        // (2 000 × 12), exactement. Vérifié à la main : 0 + 750 + 2 500 + (4 000 × 30 %) = 4 450.
        var isolatedParameters = Parameters2026 with
        {
            CnssEmployeeRate = 0m,
            ProfessionalDeductionRate = 0m,
            FamilyDeductionHeadOfHousehold = 0m,
            FamilyDeductionPerChild = 0m
        };

        var result = Engine.Calculate(new PayrollCalculationRequest(2000m, 0, NoVariables, false, 0, isolatedParameters));

        Assert.Equal(24000m, result.TaxableAnnual);
        Assert.Equal(4450m, result.IrppAnnual);
    }

    [Fact]
    public void Salaire_sous_le_seuil_exonere_n_a_pas_d_irpp()
    {
        var result = Engine.Calculate(new PayrollCalculationRequest(350m, 0, NoVariables, false, 0, Parameters2026));

        Assert.Equal(0m, result.IrppAnnual);
    }

    [Fact]
    public void La_trace_explique_chaque_montant_et_le_dernier_montant_est_le_net()
    {
        var result = Engine.Calculate(new PayrollCalculationRequest(1500m, 0, NoVariables, true, 2, Parameters2026));

        Assert.NotEmpty(result.Trace);
        Assert.Equal("Salaire net à payer", result.Trace[^1].Label);
        Assert.Equal(result.NetMonthly, result.Trace[^1].Amount);
        Assert.True(result.NetMonthly > 0 && result.NetMonthly < result.GrossMonthlySalary);
    }

    [Fact]
    public void Le_calcul_est_deterministe()
    {
        var request = new PayrollCalculationRequest(2450.500m, 0, NoVariables, true, 3, Parameters2026);

        var first = Engine.Calculate(request);
        var second = Engine.Calculate(request);

        Assert.Equal(first.NetMonthly, second.NetMonthly);
        Assert.Equal(first.IrppAnnual, second.IrppAnnual);
    }

    [Fact]
    public void Une_prime_augmente_le_brut_et_apparait_dans_la_trace()
    {
        var variables = new List<PayrollVariableInput> { new("Prime exceptionnelle", 200m) };

        var result = Engine.Calculate(new PayrollCalculationRequest(1500m, 0, variables, false, 0, Parameters2026));

        Assert.Equal(1700m, result.GrossMonthlySalary);
        Assert.Contains(result.Trace, line => line.Label == "Prime exceptionnelle" && line.Amount == 200m);
    }

    [Fact]
    public void Cinq_jours_sans_solde_reduisent_le_brut_d_un_sixieme_du_salaire_de_base()
    {
        // Convention : mois forfaitaire de 30 jours → taux journalier = base / 30.
        // 1 500 DT / 30 = 50 DT/jour ; 5 jours = 250 DT de retenue ; brut = 1 500 - 250 = 1 250 DT.
        var result = Engine.Calculate(new PayrollCalculationRequest(1500m, 5, NoVariables, false, 0, Parameters2026));

        Assert.Equal(1250m, result.GrossMonthlySalary);
    }

    [Fact]
    public void Absence_sans_solde_et_prime_se_cumulent_dans_le_meme_calcul()
    {
        var variables = new List<PayrollVariableInput> { new("Heures supplémentaires", 80m) };

        // Base 1 500, 2 jours sans solde (2 × 50 = 100 de retenue), prime 80 → brut = 1 500 - 100 + 80 = 1 480.
        var result = Engine.Calculate(new PayrollCalculationRequest(1500m, 2, variables, false, 0, Parameters2026));

        Assert.Equal(1480m, result.GrossMonthlySalary);
    }
}
