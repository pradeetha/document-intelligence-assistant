using EnterpriseAI.Api.Domain.Entities;

namespace EnterpriseAI.Api.Services;

public interface IQueryRewriter
{
    Task<string> RewriteAsync(
        IReadOnlyList<ChatMessage> history,
        string currentQuestion,
        CancellationToken cancellationToken);
}