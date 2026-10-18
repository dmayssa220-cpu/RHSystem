using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;

namespace Sirh.Application.Ai;

public sealed record WorkCertificateResult(string Content, bool Reformulated);

/// <summary>
/// Génère une attestation de travail (cahier des charges, F-02). Les faits — identité, dates,
/// type de contrat — viennent toujours directement du dossier salarié, jamais du modèle de
/// langage : celui-ci ne peut qu'en reformuler le style, jamais en changer un fait, vérifié
/// avant d'accepter sa reformulation.
/// </summary>
public sealed class HrDocumentService(IAppDbContext dbContext, ILlmClient llmClient)
{
    private const string SystemPrompt =
        "Tu es un assistant RH francophone. Reformule ce document administratif dans un style plus formel " +
        "si besoin, SANS changer un seul nom, une seule date ou un seul numéro. " +
        "Réponds uniquement avec le texte final, sans préambule.";

    public async Task<WorkCertificateResult> GenerateWorkCertificateAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken)
            ?? throw new InvalidOperationException("Salarié introuvable.");

        var contract = await dbContext.Contracts
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Aucun contrat n'est enregistré pour ce salarié.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var template =
            $"ATTESTATION DE TRAVAIL\n\n" +
            $"Je soussigné, représentant de la société, atteste que {employee.FirstName} {employee.LastName}, " +
            $"titulaire de la CIN n° {employee.NationalId}, est employé(e) au sein de notre société depuis le " +
            $"{employee.HireDate:dd/MM/yyyy}, actuellement sous contrat {contract.Type}.\n\n" +
            $"Cette attestation est délivrée à l'intéressé(e) pour servir et valoir ce que de droit.\n\n" +
            $"Fait le {today:dd/MM/yyyy}.";

        var (content, reformulated) = await TryPolishAsync(template, cancellationToken);
        return new WorkCertificateResult(content, reformulated);
    }

    private async Task<(string, bool)> TryPolishAsync(string template, CancellationToken cancellationToken)
    {
        if (!llmClient.IsConfigured)
        {
            return (template, false);
        }

        try
        {
            var polished = await llmClient.CompleteAsync(SystemPrompt, template, cancellationToken);
            return ContainsSameDates(template, polished) ? (polished.Trim(), true) : (template, false);
        }
        catch
        {
            return (template, false);
        }
    }

    private static bool ContainsSameDates(string original, string polished)
    {
        var dates = Regex.Matches(original, @"\d{2}/\d{2}/\d{4}").Select(m => m.Value);
        return dates.All(polished.Contains);
    }
}
