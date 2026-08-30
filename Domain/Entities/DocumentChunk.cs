using Microsoft.Data.SqlTypes;

namespace EnterpriseAI.Api.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public SqlVector<float> Embedding { get; set; }

    public Document Document { get; set; } = null!;
}