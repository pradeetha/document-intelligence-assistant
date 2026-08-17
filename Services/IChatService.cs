using EnterpriseAI.Api.Domain.Entities;

namespace EnterpriseAI.Api.Services;

public interface IChatService
{
    Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken);
}