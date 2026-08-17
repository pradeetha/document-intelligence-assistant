namespace EnterpriseAI.Api.Contracts.Ollama;

public record OllamaEmbeddingResponse(
    List<List<float>> Embeddings);