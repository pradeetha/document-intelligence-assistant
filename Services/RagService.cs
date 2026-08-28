using Microsoft.Data.SqlTypes;
using EnterpriseAI.Api.Data;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAI.Api.Services;

public class RagService(
    EnterpriseAiDbContext dbContext,
    IEmbeddingService embeddingService,
    IChatService chatService) : IRagService
{
    public async Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var queryEmbedding =
            await embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var sqlVector =
            new SqlVector<float>(queryEmbedding);

        var results = await dbContext.DocumentChunks
            .AsNoTracking()
            .Include(x => x.Document)
            .Select(chunk => new
            {
                Chunk = chunk,
                Distance = EF.Functions.VectorDistance(
                    "cosine",
                    chunk.Embedding,
                    sqlVector)
            })
            .Where(x => x.Distance <= 0.30)
            .OrderBy(x => x.Distance)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<IReadOnlyList<object>> DebugSearchAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var queryEmbedding =
            await embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var sqlVector =
            new SqlVector<float>(queryEmbedding);

        var results = await dbContext.DocumentChunks
            .AsNoTracking()
            .Include(x => x.Document)
            .Select(chunk => new
            {
                Chunk = chunk,
                Distance = EF.Functions.VectorDistance(
                    "cosine",
                    chunk.Embedding,
                    sqlVector)
            })
            .Where(x => x.Distance <= 0.30)
            .OrderBy(x => x.Distance)
            .Take(topK)
            .Select(x => new
            {
                FileName = x.Chunk.Document.FileName,
                ChunkIndex = x.Chunk.ChunkIndex,
                Content = x.Chunk.Content,
                Similarity = 1 - x.Distance
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<string> AskAsync(
        string query,
        IReadOnlyList<ChatMessage> history,
        int topK,
        CancellationToken cancellationToken)
    {
        var chunks = await SearchAsync(
            query,
            topK,
            cancellationToken);

        var context = string.Join(
            "\n\n",
            chunks.Select(chunk =>
                $"""
                 Document: {chunk.Document.FileName}
                 Chunk: {chunk.ChunkIndex}

                 {chunk.Content}
                 """));

        return await chatService.GetResponseAsync(
            history,
            context,
            cancellationToken);
    }
}