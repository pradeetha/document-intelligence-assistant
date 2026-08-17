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
        string context,
        CancellationToken cancellationToken)
    {
        // Start with the fixed system instruction
        var messages = new List<OllamaChatMessage>
        {
            new("system", _options.SystemPrompt)
        };
        
        var ragContext = string.IsNullOrWhiteSpace(context)
            ? "No relevant information was found in the uploaded documents."
            : context;

        messages.Add(
            new(
                "system",
                $"""
                 Answer the user's question using ONLY the information provided
                 in the knowledge base context below.

                 Knowledge base context:
                 {ragContext}

                 Rules:
                 - Do not use your general knowledge to add facts.
                 - Do not invent, assume, or infer facts that are not explicitly
                   supported by the knowledge base.
                 - If the knowledge base does not contain enough information,
                   clearly say that the information is not available.
                 - Do not combine unrelated information from different parts of
                   the knowledge base to create an unsupported conclusion.
                 - When answering, stay as close as possible to what the
                   knowledge base actually states.
                 """));
        

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