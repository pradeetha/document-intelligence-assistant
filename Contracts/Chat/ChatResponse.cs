namespace EnterpriseAI.Api.Contracts.Chat;

public record ChatResponse(
    Guid ConversationId,
    string Message,
    IReadOnlyList<ChatSource> Sources,
    string SearchQuery
    );