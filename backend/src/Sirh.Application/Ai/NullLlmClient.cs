namespace Sirh.Application.Ai;

/// <summary>Utilisée quand Ai:Provider vaut "None" (par défaut) : aucun appel réseau, jamais de coût, jamais de donnée envoyée à l'extérieur.</summary>
public sealed class NullLlmClient : ILlmClient
{
    public bool IsConfigured => false;

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Aucun fournisseur d'IA n'est configuré (variable Ai__Provider).");
}
