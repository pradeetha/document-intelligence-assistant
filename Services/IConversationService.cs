using EnterpriseAI.Api.Domain.Entities;

namespace EnterpriseAI.Api.Services;

public interface IConversationService
{
    Task<Guid> CreateConversationAsync(
        CancellationToken cancellationToken);
    
    Task AddMessageAsync(
        Guid conversationId,
        string role,
        string content,
        CancellationToken cancellationToken);

    Task<List<ChatMessage>> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
    
    Task<Conversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
    
    Task<bool> ConversationExistsAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
}