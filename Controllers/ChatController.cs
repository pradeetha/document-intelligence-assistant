using EnterpriseAI.Api.Contracts.Chat;
using EnterpriseAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IChatService chatService,
    IConversationService conversationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat(
        ChatRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        Guid conversationId;

        if (request.ConversationId is null)
        {
            conversationId =
                await conversationService.CreateConversationAsync(
                    cancellationToken);
        }
        else
        {
            conversationId = request.ConversationId.Value;

            var conversation =
                await conversationService.GetConversationAsync(
                    conversationId,
                    cancellationToken);

            if (conversation is null)
            {
                return NotFound(
                    $"Conversation '{conversationId}' was not found.");
            }
        }

        // Save user's message
        await conversationService.AddMessageAsync(
            conversationId,
            "user",
            request.Message,
            cancellationToken);

        // Get complete conversation history
        var history =
            await conversationService.GetMessagesAsync(
                conversationId,
                cancellationToken);

        // Send history to LLM
        var answer =
            await chatService.GetResponseAsync(
                history,
                cancellationToken);

        // Save AI response
        await conversationService.AddMessageAsync(
            conversationId,
            "assistant",
            answer,
            cancellationToken);

        return Ok(
            new ChatResponse(
                conversationId,
                answer));
    }
}