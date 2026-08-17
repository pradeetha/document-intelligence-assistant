using Microsoft.AspNetCore.Http;

namespace EnterpriseAI.Api.Services;

public interface IDocumentService
{
    Task<Guid> ProcessDocumentAsync(
        IFormFile file,
        CancellationToken cancellationToken);
}