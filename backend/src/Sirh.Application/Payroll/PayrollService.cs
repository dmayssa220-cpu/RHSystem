using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sirh.Application.Compliance;
using Sirh.Application.Persistence;
using Sirh.Domain.Payroll;
using Sirh.Domain.Personnel;
using Sirh.Domain.TimeOff;
using EngineNs = Sirh.Payroll.Engine;

namespace Sirh.Application.Payroll;

/// <summary>
/// Orchestration de la paie : simulation à la demande, clôture mensuelle (bulletins persistés,
/// immuables), et déclarations construites à partir des bulletins déjà clôturés — jamais
/// recalculées indépendamment, pour que la déclaration corresponde toujours aux bulletins.
/// Les absences sans solde validées et les éléments variables (primes, heures sup) du mois
/// concerné sont pris en compte automatiquement, sans ressaisie.
/// </summary>
public sealed class PayrollService(IAppDbContext dbContext, EngineNs.IPayrollEngine engine, AnomalyDetectionService anomalyDetection)
{
    public async Task<PayrollPreviewResult> PreviewAsync(PayrollPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var year = request.Year ?? today.Year;
        var month = request.Month ?? today.Month;

        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new InvalidOperationException("Salarié introuvable.");

        var contract = await dbContext.Contracts
            .Where(c => c.EmployeeId == request.EmployeeId)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Aucun contrat n'est enregistré pour ce salarié : ajoute-en un avant de simuler la paie.");

        var parameters = await GetCurrentParametersAsync(cancellationToken);
        var unpaidDays = await GetUnpaidLeaveDaysAsync(request.EmployeeId, year, month, cancellationToken);
        var variables = await GetVariablesAsync(request.EmployeeId, year, month, cancellationToken);

        var isHeadOfHousehold = request.IsHeadOfHousehold ?? employee.IsHeadOfHousehold;
        var dependentChildren = request.DependentChildren ?? employee.DependentChildren;

        var result = engine.Calculate(new EngineNs.PayrollCalculationRequest(
            contract.BaseSalary, unpaidDays, variables, isHeadOfHousehold, dependentChildren, parameters));

        return new PayrollPreviewResult(
            request.EmployeeId, year, month, result.GrossMonthlySalary, result.CnssEmployeeMonthly, result.CnssEmployerMonthly,
            result.TaxableAnnual, result.IrppAnnual, result.IrppMonthly, result.CssMonthly, result.NetMonthly,
            result.Trace.Select(t => new PayrollTraceLineDto(t.Label, t.Amount, t.Detail)).ToList());
    }

    /// <summary>
    /// Calcule et enregistre le bulletin de chaque salarié actif pour le mois donné, en tenant
    /// compte de ses absences sans solde validées et de ses éléments variables du mois. Refuse
    /// de clôturer un mois déjà clôturé (les bulletins sont immuables) et ignore silencieusement
    /// (en le signalant dans le résultat) tout salarié sans contrat.
    /// </summary>
    public async Task<CloseMonthResult> CloseMonthAsync(CloseMonthRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Month is < 1 or > 12)
        {
            throw new InvalidOperationException("Le mois doit être compris entre 1 et 12.");
        }

        var alreadyClosed = await dbContext.Payslips
            .AnyAsync(p => p.PeriodYear == request.Year && p.PeriodMonth == request.Month, cancellationToken);
        if (alreadyClosed)
        {
            throw new InvalidOperationException(
                $"Le mois {request.Month:00}/{request.Year} a déjà été clôturé. Un bulletin clôturé est immuable : " +
                "toute correction se fait par un rappel sur un mois ultérieur, jamais par une nouvelle clôture du même mois.");
        }

        var parameters = await GetCurrentParametersAsync(cancellationToken);
        var employees = await dbContext.Employees.Where(e => e.Status == EmployeeStatus.Actif).ToListAsync(cancellationToken);

        var created = new List<Payslip>();
        var skipped = 0;

        foreach (var employee in employees)
        {
            var contract = await dbContext.Contracts
                .Where(c => c.EmployeeId == employee.Id)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (contract is null)
            {
                skipped++;
                continue;
            }

            var unpaidDays = await GetUnpaidLeaveDaysAsync(employee.Id, request.Year, request.Month, cancellationToken);
            var variables = await GetVariablesAsync(employee.Id, request.Year, request.Month, cancellationToken);

            var result = engine.Calculate(new EngineNs.PayrollCalculationRequest(
                contract.BaseSalary, unpaidDays, variables, employee.IsHeadOfHousehold, employee.DependentChildren, parameters));

            created.Add(new Payslip
            {
                Id = Guid.NewGuid(),
                TenantId = employee.TenantId,
                EmployeeId = employee.Id,
                ContractId = contract.Id,
                PeriodYear = request.Year,
                PeriodMonth = request.Month,
                GrossMonthlySalary = result.GrossMonthlySalary,
                CnssEmployeeMonthly = result.CnssEmployeeMonthly,
                CnssEmployerMonthly = result.CnssEmployerMonthly,
                TaxableAnnual = result.TaxableAnnual,
                IrppAnnual = result.IrppAnnual,
                IrppMonthly = result.IrppMonthly,
                CssMonthly = result.CssMonthly,
                NetMonthly = result.NetMonthly,
                IsHeadOfHousehold = employee.IsHeadOfHousehold,
                DependentChildren = employee.DependentChildren,
                TraceJson = JsonSerializer.Serialize(result.Trace),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        dbContext.Payslips.AddRange(created);
        await dbContext.SaveChangesAsync(cancellationToken);

        var parametersEntity = await dbContext.PayrollLegalParameters
            .Where(p => p.EffectiveFrom <= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstAsync(cancellationToken);

        foreach (var payslip in created)
        {
            var alerts = await anomalyDetection.DetectAsync(payslip, parametersEntity, cancellationToken);
            dbContext.AnomalyAlerts.AddRange(alerts);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var employeesById = employees.ToDictionary(e => e.Id);
        var summaries = created
            .Select(p => new PayslipSummary(
                p.Id, p.EmployeeId,
                employeesById[p.EmployeeId].FirstName, employeesById[p.EmployeeId].LastName,
                p.PeriodYear, p.PeriodMonth, p.GrossMonthlySalary, p.NetMonthly))
            .OrderBy(s => s.EmployeeLastName)
            .ToList();

        return new CloseMonthResult(request.Year, request.Month, created.Count, skipped, summaries);
    }

    /// <summary>Déclaration Trimestrielle des Salaires (CNSS), construite à partir des bulletins déjà clôturés des 3 mois du trimestre.</summary>
    public async Task<CnssQuarterlyDeclaration> GetCnssQuarterlyDeclarationAsync(int year, int quarter, CancellationToken cancellationToken = default)
    {
        var months = QuarterMonths(quarter);

        var payslips = await dbContext.Payslips
            .Where(p => p.PeriodYear == year && months.Contains(p.PeriodMonth))
            .ToListAsync(cancellationToken);

        var employees = await GetEmployeesByIdAsync(payslips.Select(p => p.EmployeeId), cancellationToken);

        var lines = payslips
            .GroupBy(p => p.EmployeeId)
            .Select(g =>
            {
                var employee = employees.GetValueOrDefault(g.Key);
                return new CnssQuarterlyDeclarationLine(
                    g.Key, employee?.FirstName ?? "?", employee?.LastName ?? "?",
                    g.Sum(p => p.GrossMonthlySalary), g.Sum(p => p.CnssEmployeeMonthly), g.Sum(p => p.CnssEmployerMonthly));
            })
            .OrderBy(l => l.EmployeeLastName)
            .ToList();

        return new CnssQuarterlyDeclaration(
            year, quarter, QuarterlyDueDate(year, quarter), lines,
            lines.Sum(l => l.GrossQuarterly), lines.Sum(l => l.CnssEmployeeQuarterly), lines.Sum(l => l.CnssEmployerQuarterly));
    }

    /// <summary>Déclaration mensuelle de retenue à la source (IRPP + CSS), construite à partir des bulletins déjà clôturés du mois.</summary>
    public async Task<WithholdingDeclaration> GetWithholdingDeclarationAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var payslips = await dbContext.Payslips
            .Where(p => p.PeriodYear == year && p.PeriodMonth == month)
            .ToListAsync(cancellationToken);

        var employees = await GetEmployeesByIdAsync(payslips.Select(p => p.EmployeeId), cancellationToken);

        var lines = payslips
            .Select(p =>
            {
                var employee = employees.GetValueOrDefault(p.EmployeeId);
                return new WithholdingDeclarationLine(p.EmployeeId, employee?.FirstName ?? "?", employee?.LastName ?? "?", p.IrppMonthly, p.CssMonthly);
            })
            .OrderBy(l => l.EmployeeLastName)
            .ToList();

        var dueMonth = month == 12 ? 1 : month + 1;
        var dueYear = month == 12 ? year + 1 : year;

        return new WithholdingDeclaration(year, month, new DateOnly(dueYear, dueMonth, 15), lines, lines.Sum(l => l.IrppMonthly), lines.Sum(l => l.CssMonthly));
    }

    /// <summary>Un bulletin déjà clôturé, avec sa trace de calcul figée au moment de la clôture.</summary>
    public async Task<PayslipDetail?> GetPayslipAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        var payslip = await dbContext.Payslips
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PeriodYear == year && p.PeriodMonth == month, cancellationToken);

        return payslip is null ? null : ToDetail(payslip);
    }

    /// <summary>
    /// Compare deux bulletins déjà clôturés du même salarié, ligne de trace par ligne de trace,
    /// et produit une explication en langage courant des principales causes de l'écart de net —
    /// toujours construite à partir des montants réels de la trace, jamais inventée.
    /// </summary>
    public async Task<PayslipComparison> CompareAsync(Guid employeeId, int yearBefore, int monthBefore, int yearAfter, int monthAfter, CancellationToken cancellationToken = default)
    {
        var before = await dbContext.Payslips.FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PeriodYear == yearBefore && p.PeriodMonth == monthBefore, cancellationToken)
            ?? throw new InvalidOperationException($"Aucun bulletin clôturé pour {monthBefore:00}/{yearBefore}.");
        var after = await dbContext.Payslips.FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PeriodYear == yearAfter && p.PeriodMonth == monthAfter, cancellationToken)
            ?? throw new InvalidOperationException($"Aucun bulletin clôturé pour {monthAfter:00}/{yearAfter}.");

        var beforeLines = DeserializeTrace(before.TraceJson).ToDictionary(l => l.Label, l => l.Amount);
        var afterLines = DeserializeTrace(after.TraceJson).ToDictionary(l => l.Label, l => l.Amount);

        var labels = beforeLines.Keys.Union(afterLines.Keys);
        var lines = labels
            .Select(label =>
            {
                var amountBefore = beforeLines.GetValueOrDefault(label);
                var amountAfter = afterLines.GetValueOrDefault(label);
                return new PayslipComparisonLine(
                    label,
                    beforeLines.ContainsKey(label) ? amountBefore : null,
                    afterLines.ContainsKey(label) ? amountAfter : null,
                    Math.Round(amountAfter - amountBefore, 3));
            })
            .Where(l => l.Delta != 0)
            .OrderByDescending(l => Math.Abs(l.Delta))
            .ToList();

        var netDelta = Math.Round(after.NetMonthly - before.NetMonthly, 3);
        var narrative = BuildNarrative(netDelta, lines);

        return new PayslipComparison(employeeId, yearBefore, monthBefore, yearAfter, monthAfter, before.NetMonthly, after.NetMonthly, netDelta, lines, narrative);
    }

    /// <summary>
    /// Phrase d'explication construite par gabarit à partir des lignes qui ont le plus bougé —
    /// aucun appel à un modèle de langage ici : uniquement les montants réels de la trace.
    /// L'IA générative (étape suivante) pourra reformuler ce même contenu, jamais en inventer un autre.
    /// </summary>
    private static string BuildNarrative(decimal netDelta, IReadOnlyList<PayslipComparisonLine> lines)
    {
        if (netDelta == 0)
        {
            return "Le salaire net n'a pas changé par rapport au mois précédent.";
        }

        var direction = netDelta > 0 ? "augmenté" : "diminué";
        var topCauses = lines
            .Where(l => l.Label is not ("Salaire brut mensuel" or "Salaire net à payer" or "Revenu net annuel avant impôt (projection × 12)" or "Revenu net imposable annuel"))
            .Take(3)
            .Select(l => $"{l.Label} ({(l.Delta > 0 ? "+" : "")}{l.Delta:0.000} DT)")
            .ToList();

        var causesText = topCauses.Count > 0 ? " Principales causes : " + string.Join(", ", topCauses) + "." : "";

        return $"Le salaire net a {direction} de {Math.Abs(netDelta):0.000} DT par rapport au mois précédent.{causesText}";
    }

    private static List<EngineNs.PayrollTraceLine> DeserializeTrace(string json) =>
        JsonSerializer.Deserialize<List<EngineNs.PayrollTraceLine>>(json) ?? [];

    private static PayslipDetail ToDetail(Payslip payslip) => new(
        payslip.Id, payslip.EmployeeId, payslip.PeriodYear, payslip.PeriodMonth,
        payslip.GrossMonthlySalary, payslip.NetMonthly,
        DeserializeTrace(payslip.TraceJson).Select(t => new PayrollTraceLineDto(t.Label, t.Amount, t.Detail)).ToList());

    /// <summary>Nombre de jours d'absence "SansSolde" validée qui recoupent le mois donné.</summary>
    private async Task<int> GetUnpaidLeaveDaysAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken)
    {
        var periodStart = new DateOnly(year, month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var absences = await dbContext.Absences
            .Where(a => a.EmployeeId == employeeId
                     && a.Type == AbsenceType.SansSolde
                     && a.Status == AbsenceStatus.Validee
                     && a.StartDate <= periodEnd
                     && a.EndDate >= periodStart)
            .ToListAsync(cancellationToken);

        return absences.Sum(a => OverlapDays(a.StartDate, a.EndDate, periodStart, periodEnd));
    }

    private async Task<List<EngineNs.PayrollVariableInput>> GetVariablesAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken) =>
        await dbContext.PayrollVariables
            .Where(v => v.EmployeeId == employeeId && v.PeriodYear == year && v.PeriodMonth == month)
            .Select(v => new EngineNs.PayrollVariableInput(v.Label, v.Amount))
            .ToListAsync(cancellationToken);

    private static int OverlapDays(DateOnly absenceStart, DateOnly absenceEnd, DateOnly periodStart, DateOnly periodEnd)
    {
        var start = absenceStart > periodStart ? absenceStart : periodStart;
        var end = absenceEnd < periodEnd ? absenceEnd : periodEnd;
        return end.DayNumber >= start.DayNumber ? end.DayNumber - start.DayNumber + 1 : 0;
    }

    private async Task<Dictionary<Guid, Employee>> GetEmployeesByIdAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken)
    {
        var ids = employeeIds.Distinct().ToList();
        return await dbContext.Employees.Where(e => ids.Contains(e.Id)).ToDictionaryAsync(e => e.Id, cancellationToken);
    }

    private async Task<EngineNs.TunisianPayrollParameters> GetCurrentParametersAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var parameters = await dbContext.PayrollLegalParameters
            .Where(p => p.EffectiveFrom <= today)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Aucun référentiel réglementaire de paie n'est configuré.");

        return new EngineNs.TunisianPayrollParameters(
            parameters.EffectiveFrom, parameters.Source, parameters.CnssEmployeeRate, parameters.CnssEmployerRate,
            parameters.CnssCeilingAnnual, parameters.ProfessionalDeductionRate, parameters.ProfessionalDeductionCeilingAnnual,
            parameters.CssRate, parameters.FamilyDeductionHeadOfHousehold, parameters.FamilyDeductionPerChild,
            parameters.FamilyDeductionMaxChildren,
            parameters.GetBrackets().Select(b => new EngineNs.PayrollBracket(b.UpperBoundAnnual, b.Rate)).ToList());
    }

    private static int[] QuarterMonths(int quarter) => quarter switch
    {
        1 => [1, 2, 3],
        2 => [4, 5, 6],
        3 => [7, 8, 9],
        4 => [10, 11, 12],
        _ => throw new ArgumentOutOfRangeException(nameof(quarter), "Le trimestre doit être compris entre 1 et 4.")
    };

    /// <summary>Le 15 du mois suivant le trimestre échu (loi n° 60-30, art. 46).</summary>
    private static DateOnly QuarterlyDueDate(int year, int quarter)
    {
        var (dueYear, dueMonth) = quarter switch
        {
            1 => (year, 4),
            2 => (year, 7),
            3 => (year, 10),
            4 => (year + 1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(quarter), "Le trimestre doit être compris entre 1 et 4.")
        };
        return new DateOnly(dueYear, dueMonth, 15);
    }
}
