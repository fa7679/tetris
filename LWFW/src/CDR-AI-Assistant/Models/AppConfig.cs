using System.Runtime.InteropServices;

namespace CDR_AI_Assistant.Models;

public class AppConfig
{
    public string BaseURL { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o";
    public string SelectedAIProvider { get; set; } = "OpenAI";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
    public string DefaultImageSize { get; set; } = "512x512";
}