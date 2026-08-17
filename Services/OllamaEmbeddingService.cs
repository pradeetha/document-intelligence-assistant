using System.Net.Http.Json;
using EnterpriseAI.Api.Contracts.Ollama;
using EnterpriseAI.Api.Configuration;
using Microsoft.Extensions.Options;

namespace EnterpriseAI.Api.Services;

public class OllamaEmbeddingService(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options) : IEmbeddingService
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var httpClient =
            httpClientFactory.CreateClient("Ollama");

        var request = new OllamaEmbeddingRequest(
            "nomic-embed-text",
            text);

        var response = await httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl}/api/embed",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaEmbeddingResponse>(
                    cancellationToken);

        return result?.Embeddings.FirstOrDefault()?.ToArray()
               ?? throw new InvalidOperationException(
                   "Ollama returned an empty embedding.");
    }
}