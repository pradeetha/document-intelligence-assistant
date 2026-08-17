using EnterpriseAI.Api.Contracts.Conversation;
using EnterpriseAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAI.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationController(
    IConversationService conversationService)
    : ControllerBase
{
    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationResponse>> Get(
        Guid conversationId)
    {
        var conversation =
            await conversationService.GetConversationAsync(
                conversationId,
                HttpContext.RequestAborted);

        if (conversation is null)
        {
            return NotFound();
        }

        var response = new ConversationResponse(
            conversation.Id,
            conversation.CreatedAt,
            conversation.Messages
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MessageResponse(
                    x.Id,
                    x.Role,
                    x.Content,
                    x.CreatedAt))
                .ToList());

        return Ok(response);
    }
}