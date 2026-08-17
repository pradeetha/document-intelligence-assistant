namespace EnterpriseAI.Api.Contracts.Ollama;

public record OllamaChatRequest(
    string Model,
    List<OllamaChatMessage> Messages,
    bool Stream);