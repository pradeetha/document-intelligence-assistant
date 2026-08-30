using EnterpriseAI.Api.Domain.Entities;
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
    
    [HttpGet("debug-search")]
    public async Task<ActionResult> DebugSearch(
        string query,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken)
    {
        var results = await ragService.DebugSearchAsync(
            query,
            5,
            cancellationToken);

        return Ok(results);
    }
    
    [HttpGet("ask")]
    public async Task<IActionResult> Ask(
        [FromQuery] string query,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken)
    {
        var history = new List<ChatMessage>
        {
            new()
            {
                Role = "user",
                Content = query
            }
        };

        var answer = await ragService.AskAsync(
            query,
            history,
            3,
            cancellationToken);

        return Ok(new
        {
            Answer = answer
        });
    }
}
