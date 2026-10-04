using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Domain.Compliance;
using Sirh.Domain.Payroll;

namespace Sirh.Application.Compliance;

/// <summary>
/// Contrôles de conformité à règles et statistiques, exécutés automatiquement à la clôture de
/// chaque bulletin (voir Sirh.Application.Payroll.PayrollService.CloseMonthAsync). Volontairement
/// simple pour démarrer : pas encore de modèle de machine learning (voir README, section
/// Conformité et anomalies, pour la suite envisagée).
/// </summary>
public sealed class AnomalyDetectionService(IAppDbContext dbContext)
{
    /// <summary>Seuil de variation du brut, d'un mois sur l'autre, à partir duquel une alerte statistique est levée.</summary>
    private const decimal SignificantVariationThreshold = 0.20m;

    /// <summary>Analyse un bulletin qui vient d'être créé et retourne les alertes à enregistrer (ne les enregistre pas elle-même).</summary>
    public async Task<List<AnomalyAlert>> DetectAsync(Payslip payslip, PayrollLegalParameters parameters, CancellationToken cancellationToken = default)
    {
        var alerts = new List<AnomalyAlert>();

        if (payslip.NetMonthly <= 0)
        {
            alerts.Add(NewAlert(payslip, "NetNegatifOuNul", AnomalySeverity.Eleve,
                $"Le salaire net calculé ({payslip.NetMonthly:0.000} DT) est négatif ou nul."));
        }

        if (parameters.MinimumWageMonthly > 0 && payslip.GrossMonthlySalary < parameters.MinimumWageMonthly)
        {
            alerts.Add(NewAlert(payslip, "SalaireInferieurAuSmig", AnomalySeverity.Eleve,
                $"Le salaire brut ({payslip.GrossMonthlySalary:0.000} DT) est inférieur au SMIG retenu ({parameters.MinimumWageMonthly:0.000} DT, régime 48h — à vérifier si le salarié est au régime 40h)."));
        }

        var (previousYear, previousMonth) = payslip.PeriodMonth == 1
            ? (payslip.PeriodYear - 1, 12)
            : (payslip.PeriodYear, payslip.PeriodMonth - 1);

        var previous = await dbContext.Payslips
            .FirstOrDefaultAsync(p => p.EmployeeId == payslip.EmployeeId && p.PeriodYear == previousYear && p.PeriodMonth == previousMonth, cancellationToken);

        if (previous is not null && previous.GrossMonthlySalary > 0)
        {
            var variation = (payslip.GrossMonthlySalary - previous.GrossMonthlySalary) / previous.GrossMonthlySalary;
            if (Math.Abs(variation) >= SignificantVariationThreshold)
            {
                var direction = variation > 0 ? "hausse" : "baisse";
                alerts.Add(NewAlert(payslip, "VariationBrutImportante", AnomalySeverity.Moyen,
                    $"Le salaire brut varie de {variation:+0.0%;-0.0%} par rapport au mois précédent ({previous.GrossMonthlySalary:0.000} DT → {payslip.GrossMonthlySalary:0.000} DT, {direction}) : à vérifier (prime exceptionnelle, erreur de saisie, changement de contrat…)."));
            }
        }

        return alerts;
    }

    public async Task<IReadOnlyList<AnomalyAlertSummary>> ListAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = from a in dbContext.AnomalyAlerts
                     join e in dbContext.Employees on a.EmployeeId equals e.Id
                     select new { Alert = a, Employee = e };

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsedStatus = Enum.Parse<AnomalyStatus>(status, ignoreCase: true);
            query = query.Where(x => x.Alert.Status == parsedStatus);
        }

        var results = await query
            .OrderByDescending(x => x.Alert.CreatedAtUtc)
            .Select(x => new AnomalyAlertSummary(
                x.Alert.Id, x.Alert.PayslipId, x.Alert.EmployeeId, x.Employee.FirstName, x.Employee.LastName,
                x.Alert.PeriodYear, x.Alert.PeriodMonth, x.Alert.RuleCode, x.Alert.Severity.ToString(),
                x.Alert.Message, x.Alert.Status.ToString(), x.Alert.DecisionComment))
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<AnomalyAlertSummary?> DecideAsync(Guid id, DecideAnomalyRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await dbContext.AnomalyAlerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alert is null)
        {
            return null;
        }

        alert.Status = request.Confirm ? AnomalyStatus.Confirmee : AnomalyStatus.FauxPositif;
        alert.DecisionComment = request.Comment;
        alert.DecidedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var employee = await dbContext.Employees.FirstAsync(e => e.Id == alert.EmployeeId, cancellationToken);
        return new AnomalyAlertSummary(
            alert.Id, alert.PayslipId, alert.EmployeeId, employee.FirstName, employee.LastName,
            alert.PeriodYear, alert.PeriodMonth, alert.RuleCode, alert.Severity.ToString(),
            alert.Message, alert.Status.ToString(), alert.DecisionComment);
    }

    private static AnomalyAlert NewAlert(Payslip payslip, string ruleCode, AnomalySeverity severity, string message) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = payslip.TenantId,
        PayslipId = payslip.Id,
        EmployeeId = payslip.EmployeeId,
        PeriodYear = payslip.PeriodYear,
        PeriodMonth = payslip.PeriodMonth,
        RuleCode = ruleCode,
        Severity = severity,
        Message = message,
        Status = AnomalyStatus.Detectee,
        CreatedAtUtc = DateTime.UtcNow
    };
}
