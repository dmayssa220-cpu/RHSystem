using System.Text.Json;
using Sirh.Application.Ai;

namespace Sirh.Api.Ai;

/// <summary>
/// Appelle un serveur Ollama (modèle de langage local, gratuit, voir https://ollama.com) pour
/// obtenir une reformulation. N'est utilisée que si Ai:Provider=Ollama est configuré — voir
/// README, section IA générative, pour l'activer.
/// </summary>
public sealed class OllamaLlmClient(HttpClient httpClient, IConfiguration configuration) : ILlmClient
{
    public bool IsConfigured => true;

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var model = configuration["Ai:OllamaModel"] ?? "llama3.2";

        var payload = new
        {
            model,
            prompt = $"{systemPrompt}\n\n{userPrompt}",
            stream = false
        };

        using var response = await httpClient.PostAsJsonAsync("/api/generate", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("response").GetString() ?? string.Empty;
    }
}
