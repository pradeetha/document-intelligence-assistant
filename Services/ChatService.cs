using System.Net.Http.Json;
using EnterpriseAI.Api.Configuration;
using EnterpriseAI.Api.Contracts.Ollama;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.Extensions.Options;

namespace EnterpriseAI.Api.Services;

public class ChatService(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options) : IChatService
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken)
    {
        // Start with the fixed system instruction
        var messages = new List<OllamaChatMessage>
        {
            new("system", _options.SystemPrompt)
        };

        // Convert our database messages into Ollama messages
        messages.AddRange(
            history.Select(message =>
                new OllamaChatMessage(
                    message.Role,
                    message.Content)));

        var httpClient = httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _options.Model,
            messages,
            false);

        var response = await httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl}/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken);

        return result?.Message.Content
               ?? throw new InvalidOperationException(
                   "Ollama returned an empty response.");
    }
}