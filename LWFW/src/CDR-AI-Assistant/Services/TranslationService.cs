using System.IO;
using CDR_AI_Assistant.Services.AIServices;

namespace CDR_AI_Assistant.Services;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public TranslationService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> TranslateAsync(string text, string targetLang = "ZH")
    {
        var cfg = _config.Load();

        var requestBody = new
        {
            text = text,
            target_lang = targetLang
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestBody),
            System.Text.Encoding.UTF8,
            "application/json");

        var apiKey = string.IsNullOrEmpty(cfg.EncryptedApiKey)
            ? cfg.ApiKey
            : _config.Decrypt(cfg.EncryptedApiKey);

        var response = await _httpClient.PostAsync(
            $"https://api-free.deepl.com/v2/translate?auth_key={apiKey}",
            content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json)
            ?? throw new InvalidOperationException("翻译解析失败");

        return result.translations[0].text.ToString();
    }
}