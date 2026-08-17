using EnterpriseAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAI.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController(
    IDocumentService documentService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var documentId =
            await documentService.ProcessDocumentAsync(
                file,
                cancellationToken);

        return Ok(new
        {
            DocumentId = documentId
        });
    }
}