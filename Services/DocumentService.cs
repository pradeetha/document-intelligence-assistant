using System.Text.Json;
using EnterpriseAI.Api.Data;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace EnterpriseAI.Api.Services;

public class DocumentService(
    EnterpriseAiDbContext dbContext,
    IEmbeddingService embeddingService) : IDocumentService
{
    public async Task<Guid> ProcessDocumentAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("The uploaded file is empty.");
        }

        if (!file.FileName.EndsWith(
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only PDF files are supported.");
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            CreatedAt = DateTime.UtcNow
        };

        await using var stream = file.OpenReadStream();

        using var pdf = PdfDocument.Open(stream);

        var fullText = string.Join(
            Environment.NewLine,
            pdf.GetPages().Select(page => page.Text));

        var chunks = CreateChunks(fullText);

        foreach (var chunk in chunks)
        {
            var embedding =
                await embeddingService.GenerateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            document.Chunks.Add(
                new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    Content = chunk.Content,
                    ChunkIndex = chunk.Index,
                    Embedding = JsonSerializer.Serialize(embedding)
                });
        }

        dbContext.Documents.Add(document);

        await dbContext.SaveChangesAsync(cancellationToken);

        return document.Id;
    }

    private static List<(int Index, string Content)> CreateChunks(
        string text)
    {
        const int chunkSize = 1000;

        var chunks = new List<(int, string)>();

        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var length = Math.Min(
                chunkSize,
                text.Length - i);

            chunks.Add(
                (chunks.Count, text.Substring(i, length)));
        }

        return chunks;
    }
}