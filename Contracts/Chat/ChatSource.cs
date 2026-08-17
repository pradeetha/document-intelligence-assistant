namespace EnterpriseAI.Api.Contracts.Chat;

public record ChatSource(
    string FileName,
    int ChunkIndex);