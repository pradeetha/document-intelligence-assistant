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
        const int maxChunkSize = 1000;
        const int overlapSentences = 1;

        var sentences = text
            .Split(
                ['.', '!', '?'],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x + ".")
            .ToList();

        var chunks = new List<(int Index, string Content)>();

        var currentSentences = new List<string>();
        var currentLength = 0;

        foreach (var sentence in sentences)
        {
            if (currentLength + sentence.Length > maxChunkSize
                && currentSentences.Count > 0)
            {
                var chunkContent = string.Join(
                    " ",
                    currentSentences);

                chunks.Add(
                    (
                        chunks.Count,
                        chunkContent
                    ));

                // Keep the last sentence as overlap.
                var overlap = currentSentences
                    .TakeLast(overlapSentences)
                    .ToList();

                currentSentences = overlap;
                currentLength = overlap.Sum(x => x.Length);
            }

            currentSentences.Add(sentence);
            currentLength += sentence.Length;
        }

        if (currentSentences.Count > 0)
        {
            chunks.Add(
                (
                    chunks.Count,
                    string.Join(" ", currentSentences)
                ));
        }

        return chunks;
    }
}