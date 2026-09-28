using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using EngineNs = Sirh.Payroll.Engine;

namespace Sirh.Application.Payroll;

/// <summary>
/// Prépare l'appel au moteur de paie (Sirh.Payroll.Engine) : va chercher le contrat du
/// salarié et le référentiel réglementaire en vigueur, les convertit en entrées du moteur,
/// puis restitue le résultat et sa trace de calcul.
/// </summary>
public sealed class PayrollService(IAppDbContext dbContext, EngineNs.IPayrollEngine engine)
{
    public async Task<PayrollPreviewResult> PreviewAsync(PayrollPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await dbContext.Contracts
            .Where(c => c.EmployeeId == request.EmployeeId)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Aucun contrat n'est enregistré pour ce salarié : ajoute-en un avant de simuler la paie.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var parameters = await dbContext.PayrollLegalParameters
            .Where(p => p.EffectiveFrom <= today)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Aucun référentiel réglementaire de paie n'est configuré.");

        var engineParameters = new EngineNs.TunisianPayrollParameters(
            parameters.EffectiveFrom,
            parameters.Source,
            parameters.CnssEmployeeRate,
            parameters.CnssEmployerRate,
            parameters.CnssCeilingAnnual,
            parameters.ProfessionalDeductionRate,
            parameters.ProfessionalDeductionCeilingAnnual,
            parameters.CssRate,
            parameters.FamilyDeductionHeadOfHousehold,
            parameters.FamilyDeductionPerChild,
            parameters.FamilyDeductionMaxChildren,
            parameters.GetBrackets().Select(b => new EngineNs.PayrollBracket(b.UpperBoundAnnual, b.Rate)).ToList());

        var result = engine.Calculate(new EngineNs.PayrollCalculationRequest(
            contract.BaseSalary, request.IsHeadOfHousehold, request.DependentChildren, engineParameters));

        return new PayrollPreviewResult(
            request.EmployeeId,
            result.GrossMonthlySalary,
            result.CnssEmployeeMonthly,
            result.CnssEmployerMonthly,
            result.TaxableAnnual,
            result.IrppAnnual,
            result.IrppMonthly,
            result.CssMonthly,
            result.NetMonthly,
            result.Trace.Select(t => new PayrollTraceLineDto(t.Label, t.Amount, t.Detail)).ToList());
    }
}
