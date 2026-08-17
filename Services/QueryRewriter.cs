using System.Net.Http.Json;
using EnterpriseAI.Api.Configuration;
using EnterpriseAI.Api.Contracts.Ollama;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.Extensions.Options;

namespace EnterpriseAI.Api.Services;

public class QueryRewriter(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options) : IQueryRewriter
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> RewriteAsync(
        IReadOnlyList<ChatMessage> history,
        string currentQuestion,
        CancellationToken cancellationToken)
    {
        if (!NeedsRewriting(history, currentQuestion))
        {
            return currentQuestion;
        }
        
        var conversation = string.Join(
            "\n",
            history.Select(message =>
                $"{message.Role}: {message.Content}"));

        var messages = new List<OllamaChatMessage>
        {
            new(
                "system",
                """
                Rewrite the user's latest question into a standalone search query
                for a document knowledge base.

                Your ONLY job is to resolve references from the conversation history
                and make the latest question understandable without the conversation.

                Rules:
                - Preserve the exact intent of the latest question.
                - Resolve references such as "it", "that", "they", "them", "those",
                  and similar references using the conversation history.
                - Keep the same subject, scope, and requested information.
                - Do not broaden the question.
                - Do not narrow the question.
                - Do not add information that the user did not ask for.
                - Do not answer the question.
                - Do not explain your reasoning.
                - Return ONLY the standalone search query.
                """),

            new(
                "user",
                $"""
                Conversation history:
                {conversation}

                Latest question:
                {currentQuestion}
                """)
        };

        var httpClient =
            httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _options.Model,
            messages,
            false);

        var response = await httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl}/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken);

        return result?.Message.Content
                   .Trim()
                   .Trim('"')
               ?? throw new InvalidOperationException(
                   "Ollama returned an empty query.");
    }
    
    private static bool NeedsRewriting(
        IReadOnlyList<ChatMessage> history,
        string currentQuestion)
    {
        if (history.Count <= 1)
        {
            return false;
        }

        var referenceWords = new[]
        {
            "it",
            "that",
            "this",
            "they",
            "them",
            "those",
            "these",
            "he",
            "she",
            "there",
            "then"
        };

        var words = currentQuestion
            .ToLowerInvariant()
            .Split(
                [' ', ',', '.', '?', '!', ';', ':'],
                StringSplitOptions.RemoveEmptyEntries);

        return words.Any(word =>
            referenceWords.Contains(word));
    }
}