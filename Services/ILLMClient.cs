namespace EnterpriseAI.Api.Services;

public interface ILLMClient
{
    Task<string> GenerateResponseAsync(string message);
}