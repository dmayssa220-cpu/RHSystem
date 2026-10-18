using System.Text.RegularExpressions;

namespace Sirh.Application.Ai;

/// <summary>
/// Reformule une explication de paie déjà construite par gabarit (Sirh.Application.Payroll.PayrollService,
/// comparaison inter-périodes). Ne reçoit jamais la trace brute à "halluciner" : seulement le texte
/// déjà correct, à rendre plus naturel. Si aucun fournisseur n'est configuré, ou si la reformulation
/// a fait disparaître un montant présent dans le texte d'origine, le texte par gabarit est renvoyé
/// tel quel — aucun montant inventé n'est jamais affiché.
/// </summary>
public sealed class PayrollExplanationAgent(ILlmClient llmClient)
{
    private const string SystemPrompt =
        "Tu es un assistant RH francophone. Reformule le texte suivant en français courant, de façon " +
        "plus naturelle et chaleureuse, sans jamais changer, ajouter ou supprimer un montant en dinars. " +
        "Réponds uniquement avec le texte reformulé, sans préambule ni guillemets.";

    public async Task<(string Text, bool Reformulated)> ExplainAsync(string templateNarrative, CancellationToken cancellationToken = default)
    {
        if (!llmClient.IsConfigured)
        {
            return (templateNarrative, false);
        }

        try
        {
            var reformulated = await llmClient.CompleteAsync(SystemPrompt, templateNarrative, cancellationToken);
            return AmountsMatch(templateNarrative, reformulated) ? (reformulated.Trim(), true) : (templateNarrative, false);
        }
        catch
        {
            return (templateNarrative, false);
        }
    }

    private static bool AmountsMatch(string original, string reformulated)
    {
        var originalAmounts = ExtractAmounts(original);
        var reformulatedAmounts = ExtractAmounts(reformulated);
        return originalAmounts.All(reformulatedAmounts.Contains);
    }

    private static HashSet<string> ExtractAmounts(string text) =>
        Regex.Matches(text, @"\d+[.,]\d{3}").Select(m => m.Value.Replace(',', '.')).ToHashSet();
}
