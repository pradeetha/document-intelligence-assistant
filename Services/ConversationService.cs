using EnterpriseAI.Api.Data;
using EnterpriseAI.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAI.Api.Services;

public class ConversationService(
    EnterpriseAiDbContext dbContext) : IConversationService
{
    public async Task<Guid> CreateConversationAsync(
        CancellationToken cancellationToken)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Conversations.Add(conversation);

        await dbContext.SaveChangesAsync(cancellationToken);

        return conversation.Id;
    }
    
    public async Task AddMessageAsync(
        Guid conversationId,
        string role,
        string content,
        CancellationToken cancellationToken)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ChatMessages.Add(message);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ChatMessages
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Conversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(
                x => x.Id == conversationId,
                cancellationToken);
    }
    
    public async Task<bool> ConversationExistsAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AnyAsync(
                x => x.Id == conversationId,
                cancellationToken);
    }
}