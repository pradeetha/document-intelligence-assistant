namespace EnterpriseAI.Api.Contracts.Chat;

public record ChatRequest(
    string Message,
    Guid? ConversationId);