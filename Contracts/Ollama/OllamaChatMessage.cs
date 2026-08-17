namespace EnterpriseAI.Api.Contracts.Ollama;

public record OllamaChatMessage(
    string Role,
    string Content);