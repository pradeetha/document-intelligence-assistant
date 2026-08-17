using System.Text.Json;
using EnterpriseAI.Api.Data;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAI.Api.Services;

public class RagService(
    EnterpriseAiDbContext dbContext,
    IEmbeddingService embeddingService) : IRagService
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

        var chunks = await dbContext.DocumentChunks
            .AsNoTracking()
            .Include(x => x.Document)
            .Where(x => x.Embedding != "")
            .ToListAsync(cancellationToken);

        var results = chunks
            .Select(chunk =>
            {
                var embedding =
                    JsonSerializer.Deserialize<float[]>(
                        chunk.Embedding)!;

                var similarity =
                    CosineSimilarity(
                        queryEmbedding,
                        embedding);

                return new
                {
                    Chunk = chunk,
                    Similarity = similarity
                };
            })
            .Where(x => x.Similarity >= 0.4)
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();

        return results;
    }

    private static double CosineSimilarity(
        float[] a,
        float[] b)
    {
        if (a.Length != b.Length)
        {
            throw new InvalidOperationException(
                "Embedding dimensions do not match.");
        }

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];

            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct /
               (Math.Sqrt(magnitudeA) *
                Math.Sqrt(magnitudeB));
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

        var chunks = await dbContext.DocumentChunks
            .AsNoTracking()
            .Include(x => x.Document)
            .Where(x => x.Embedding != "")
            .ToListAsync(cancellationToken);

        var results = chunks
            .Select(chunk =>
            {
                var embedding =
                    JsonSerializer.Deserialize<float[]>(
                        chunk.Embedding)!;

                var similarity =
                    CosineSimilarity(
                        queryEmbedding,
                        embedding);

                return new
                {
                    FileName = chunk.Document.FileName,
                    ChunkIndex = chunk.ChunkIndex,
                    Content = chunk.Content,
                    Similarity = similarity
                };
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Cast<object>()
            .ToList();

        return results;
    }
}