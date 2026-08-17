namespace EnterpriseAI.Api.Services;

public class FakeLLMClient
{
    public Task<string> GenerateResponseAsync(string message)
    {
        var response =
            $"The AI received your message: {message}";

        return Task.FromResult(response);
    }
}