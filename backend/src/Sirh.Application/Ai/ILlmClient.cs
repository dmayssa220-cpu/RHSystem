namespace Sirh.Application.Ai;

/// <summary>
/// Passerelle vers un modèle de langage, abstraite de tout fournisseur (voir README, section
/// IA générative, pour le principe : l'IA reformule, elle ne calcule et n'invente jamais un fait).
/// Implémentée par NullLlmClient (aucun fournisseur configuré, par défaut) ou par un vrai
/// fournisseur côté Sirh.Api (ex. OllamaLlmClient, local et gratuit).
/// </summary>
public interface ILlmClient
{
    /// <summary>Faux si aucun fournisseur n'est configuré : les agents doivent alors se rabattre sur un texte par gabarit, jamais échouer.</summary>
    bool IsConfigured { get; }

    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
