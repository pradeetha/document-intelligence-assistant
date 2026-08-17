namespace EnterpriseAI.Api.Contracts.Conversation;

public record MessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);