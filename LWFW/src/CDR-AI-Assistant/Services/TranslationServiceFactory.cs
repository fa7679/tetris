using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class TranslationServiceFactory
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public TranslationServiceFactory(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public ITranslationService GetService(string? providerName = null)
    {
        return providerName?.ToLower() switch
        {
            "google" => new AIServices.GoogleTranslateService(_httpClient, _config),
            "deepl" => new DeepLTranslateService(_httpClient, _config),
            _ => new DeepLTranslateService(_httpClient, _config)
        };
    }

    public static IEnumerable<string> GetAvailableProviders()
    {
        return new[] { "DeepL", "Google Translate" };
    }
}