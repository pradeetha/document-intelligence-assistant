namespace EnterpriseAI.Api.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string Embedding { get; set; } = string.Empty;

    public Document Document { get; set; } = null!;
}