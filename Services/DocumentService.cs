using System.Text.Json;
using EnterpriseAI.Api.Data;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.Data.SqlTypes;
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
            
            Console.WriteLine($"Embedding dimensions: {embedding.Length}");
            
            document.Chunks.Add(
                new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    Content = chunk.Content,
                    ChunkIndex = chunk.Index,
                    Embedding = new SqlVector<float>(embedding)
                });
        }

        dbContext.Documents.Add(document);

        await dbContext.SaveChangesAsync(cancellationToken);

        return document.Id;
    }

    private static List<(int Index, string Content)> CreateChunks(
        string text)
    {
        const int maxChunkSize = 600;
        const int overlapSize = 100;

        var cleanedText = text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();

        var chunks = new List<(int Index, string Content)>();

        var start = 0;

        while (start < cleanedText.Length)
        {
            var targetEnd = Math.Min(
                start + maxChunkSize,
                cleanedText.Length);

            var end = targetEnd;

            // Prefer a natural break near the target end.
            if (targetEnd < cleanedText.Length)
            {
                var lastSpace = cleanedText.LastIndexOf(
                    ' ',
                    targetEnd - 1,
                    maxChunkSize);

                if (lastSpace > start)
                {
                    end = lastSpace;
                }
            }

            var content = cleanedText[start..end].Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                chunks.Add(
                    (
                        chunks.Count,
                        content
                    ));
            }

            if (end >= cleanedText.Length)
            {
                break;
            }

            // Move forward while retaining overlap.
            start = end - overlapSize;

            // Safety check to guarantee progress.
            if (start <= 0)
            {
                start = end;
            }
        }

        return chunks;
    }
}