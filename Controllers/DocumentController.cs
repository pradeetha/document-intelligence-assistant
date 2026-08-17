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
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken)
    {
        var results = await ragService.SearchAsync(
            query,
            3,
            cancellationToken);

        return Ok(results.Select(x => new
        {
            x.Id,
            x.Content,
            x.ChunkIndex
        }));
    }
}
