using Sirh.Payroll.Engine;

namespace Sirh.Payroll.Tests;

public class TunisianPayrollEngineTests
{
    
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

        var result = Engine.Calculate(new PayrollCalculationRequest(2000m, false, 0, isolatedParameters));

        Assert.Equal(24000m, result.TaxableAnnual);
        Assert.Equal(4450m, result.IrppAnnual);
    }

    [Fact]
    public void Salaire_sous_le_seuil_exonere_n_a_pas_d_irpp()
    {
        var result = Engine.Calculate(new PayrollCalculationRequest(
            GrossMonthlySalary: 350m, IsHeadOfHousehold: false, DependentChildren: 0, Parameters: Parameters2026));

        Assert.Equal(0m, result.IrppAnnual);
    }

    [Fact]
    public void La_trace_explique_chaque_montant_et_le_dernier_montant_est_le_net()
    {
        var result = Engine.Calculate(new PayrollCalculationRequest(
            GrossMonthlySalary: 1500m, IsHeadOfHousehold: true, DependentChildren: 2, Parameters: Parameters2026));

        Assert.NotEmpty(result.Trace);
        Assert.Equal("Salaire net à payer", result.Trace[^1].Label);
        Assert.Equal(result.NetMonthly, result.Trace[^1].Amount);
        Assert.True(result.NetMonthly > 0 && result.NetMonthly < result.GrossMonthlySalary);
    }

    [Fact]
    public void Le_calcul_est_deterministe()
    {
        var request = new PayrollCalculationRequest(2450.500m, true, 3, Parameters2026);

        var first = Engine.Calculate(request);
        var second = Engine.Calculate(request);

        Assert.Equal(first.NetMonthly, second.NetMonthly);
        Assert.Equal(first.IrppAnnual, second.IrppAnnual);
    }
}
