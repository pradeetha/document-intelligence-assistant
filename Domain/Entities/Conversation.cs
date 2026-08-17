namespace EnterpriseAI.Api.Domain.Entities;


public class Conversation
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];
}