using EnterpriseAI.Api.Domain.Entities;

namespace EnterpriseAI.Api.Services;

public interface IRagService
{
    Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        string query,
        int topK,
        CancellationToken cancellationToken);
}