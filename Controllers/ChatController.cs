using EnterpriseAI.Api.Contracts.Chat;
using EnterpriseAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IChatService chatService,
    IConversationService conversationService,
    IRagService ragService) : ControllerBase
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
        
        var relevantChunks =
            await ragService.SearchAsync(
                request.Message,
                3,
                cancellationToken);

        if (relevantChunks.Count == 0)
        {
            return Ok(
                new ChatResponse(
                    conversationId,
                    "I couldn't find relevant information in the uploaded documents.", []));
        }
        
        var sources = relevantChunks
            .Select(chunk =>
                new ChatSource(
                    chunk.Document.FileName,
                    chunk.ChunkIndex))
            .ToList();
        
        var context = string.Join(
            "\n\n",
            relevantChunks.Select(chunk =>
            $"""
             Source: {chunk.Document.FileName}
             Chunk: {chunk.ChunkIndex}

             {chunk.Content}
             """));

        // Send history to LLM
        var answer =
            await chatService.GetResponseAsync(
                history,
                context,
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
                answer,
                sources));
    }
}