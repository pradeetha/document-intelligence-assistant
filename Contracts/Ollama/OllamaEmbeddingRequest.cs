namespace EnterpriseAI.Api.Contracts.Ollama;

public record OllamaEmbeddingRequest(
    string Model,
    string Input);