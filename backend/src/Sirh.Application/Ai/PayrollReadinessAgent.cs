using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Domain.Personnel;
using Sirh.Domain.TimeOff;

namespace Sirh.Application.Ai;

public sealed record PayrollReadinessIssue(string Category, Guid? EmployeeId, string Message);

public sealed record PayrollReadinessReport(
    int Year, int Month, int ActiveEmployeeCount, int ReadyEmployeeCount,
    IReadOnlyList<PayrollReadinessIssue> Issues, string Summary, bool SummaryReformulated);

/// <summary>
/// Agent de préparation de paie (cahier des charges, G-01) : vérifie la complétude des données
/// avant une clôture et signale les cas incomplets. Ne modifie jamais rien lui-même — seulement
/// le gestionnaire décide, à partir de ce rapport, de clôturer ou d'attendre.
/// </summary>
public sealed class PayrollReadinessAgent(IAppDbContext dbContext, ILlmClient llmClient)
{
    private const string SystemPrompt =
        "Tu es un assistant RH francophone. Résume ce rapport de préparation de paie en 2 ou 3 phrases " +
        "claires, en français, sans ajouter aucune information qui n'y figure pas déjà. " +
        "Réponds uniquement avec le résumé, sans préambule.";

    public async Task<PayrollReadinessReport> CheckAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var periodStart = new DateOnly(year, month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var employees = await dbContext.Employees.Where(e => e.Status == EmployeeStatus.Actif).ToListAsync(cancellationToken);
        var issues = new List<PayrollReadinessIssue>();

        foreach (var employee in employees)
        {
            var hasContract = await dbContext.Contracts.AnyAsync(c => c.EmployeeId == employee.Id, cancellationToken);
            if (!hasContract)
            {
                issues.Add(new PayrollReadinessIssue("ContratManquant", employee.Id, $"{employee.LastName} {employee.FirstName} n'a aucun contrat enregistré."));
            }
        }

        var pendingAbsences = await dbContext.Absences
            .Where(a => a.Status == AbsenceStatus.EnAttente && a.StartDate <= periodEnd && a.EndDate >= periodStart)
            .ToListAsync(cancellationToken);

        foreach (var absence in pendingAbsences)
        {
            var employee = employees.FirstOrDefault(e => e.Id == absence.EmployeeId);
            var name = employee is not null ? $"{employee.LastName} {employee.FirstName}" : "un salarié";
            issues.Add(new PayrollReadinessIssue("AbsenceEnAttente", absence.EmployeeId, $"Une demande d'absence de {name} sur cette période n'est pas encore validée."));
        }

        var alreadyClosed = await dbContext.Payslips.AnyAsync(p => p.PeriodYear == year && p.PeriodMonth == month, cancellationToken);
        if (alreadyClosed)
        {
            issues.Add(new PayrollReadinessIssue("DejaCloture", null, $"Le mois {month:00}/{year} a déjà été clôturé."));
        }

        var readyCount = employees.Count - issues.Count(i => i.Category == "ContratManquant");

        var templateSummary = issues.Count == 0
            ? $"Les {employees.Count} salarié(s) actif(s) sont prêts pour la clôture de {month:00}/{year}."
            : $"{issues.Count} point(s) à vérifier avant de clôturer {month:00}/{year} : " + string.Join(" ", issues.Select(i => i.Message));

        var (summary, reformulated) = await TryReformulateAsync(templateSummary, cancellationToken);

        return new PayrollReadinessReport(year, month, employees.Count, readyCount, issues, summary, reformulated);
    }

    private async Task<(string Text, bool Reformulated)> TryReformulateAsync(string template, CancellationToken cancellationToken)
    {
        if (!llmClient.IsConfigured)
        {
            return (template, false);
        }

        try
        {
            var reformulated = await llmClient.CompleteAsync(SystemPrompt, template, cancellationToken);
            return (reformulated.Trim(), true);
        }
        catch
        {
            return (template, false);
        }
    }
}
