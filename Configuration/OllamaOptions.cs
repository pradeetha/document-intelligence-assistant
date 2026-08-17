namespace EnterpriseAI.Api.Configuration;

public class OllamaOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
    
    public string SystemPrompt { get; set; } = string.Empty;
}