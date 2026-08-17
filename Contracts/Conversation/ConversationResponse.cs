namespace EnterpriseAI.Api.Contracts.Conversation;

public record ConversationResponse(
    Guid Id,
    DateTime CreatedAt,
    IReadOnlyList<MessageResponse> Messages);