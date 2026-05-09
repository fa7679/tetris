using System.IO;
using CDR_AI_Assistant.Services;
using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class AIServiceFactory
{
    private readonly ConfigManager _config;
    private readonly HttpClient _httpClient;

    public AIServiceFactory(ConfigManager config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public IAIService GetService(string? providerName = null)
    {
        var cfg = _config.Load();
        providerName ??= cfg.SelectedAIProvider ?? "OpenAI";

        return providerName.ToLower() switch
        {
            "openai" => new AIServices.OpenAIService(_httpClient, _config),
            "claude" => new AIServices.ClaudeService(_httpClient, _config),
            "stable diffusion" => new AIServices.StableDiffusionService(_httpClient, _config),
            _ => new AIServices.OpenAIService(_httpClient, _config)
        };
    }

    public IImageGeneratorService? GetImageService()
    {
        var cfg = _config.Load();
        return cfg.SelectedAIProvider?.ToLower() switch
        {
            "stable diffusion" => new AIServices.StableDiffusionService(_httpClient, _config),
            "midjourney" => new AIServices.MidjourneyService(_httpClient, _config),
            _ => null
        };
    }

    public static IEnumerable<string> GetAvailableProviders()
    {
        return new[] { "OpenAI", "Claude", "Stable Diffusion", "Midjourney" };
    }

    public static IEnumerable<string> GetImageProviders()
    {
        return new[] { "Stable Diffusion", "Midjourney" };
    }
}