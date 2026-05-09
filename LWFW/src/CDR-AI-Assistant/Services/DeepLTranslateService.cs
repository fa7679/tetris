using CDR_AI_Assistant.Services.Interfaces;

namespace CDR_AI_Assistant.Services;

public class DeepLTranslateService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "DeepL";

    public DeepLTranslateService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> TranslateAsync(string text, string targetLang = "ZH", CancellationToken ct = default)
    {
        var cfg = _config.Load();

        var apiKey = !string.IsNullOrEmpty(cfg.DeepLApiKeyEncrypted)
            ? _config.Decrypt(cfg.DeepLApiKeyEncrypted)
            : cfg.DeepLApiKey;

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("DeepL API Key 未配置，请在设置中配置");

        var langCode = MapToDeepLLangCode(targetLang);

        var requestBody = new
        {
            text = text,
            target_lang = langCode
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestBody),
            System.Text.Encoding.UTF8,
            "application/json");

        var url = $"https://api-free.deepl.com/v2/translate?auth_key={apiKey}";
        var response = await _httpClient.PostAsync(url, content, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json)
            ?? throw new InvalidOperationException("翻译解析失败");

        return result.translations[0].text.ToString();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var cfg = _config.Load();
            var apiKey = !string.IsNullOrEmpty(cfg.DeepLApiKeyEncrypted)
                ? _config.Decrypt(cfg.DeepLApiKeyEncrypted)
                : cfg.DeepLApiKey;

            if (string.IsNullOrEmpty(apiKey))
                return false;

            var testText = new[] { new { text = "Hello", target_lang = "DE" } };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new { text = "Hello", target_lang = "DE" }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"https://api-free.deepl.com/v2/translate?auth_key={apiKey}",
                content,
                new CancellationTokenSource(5000).Token);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string MapToDeepLLangCode(string code) => code switch
    {
        "ZH" => "ZH",
        "EN" => "EN",
        "JA" => "JA",
        "KO" => "KO",
        "FR" => "FR",
        "DE" => "DE",
        "ES" => "ES",
        "IT" => "IT",
        "NL" => "NL",
        "PL" => "PL",
        "PT" => "PT",
        "RU" => "RU",
        "SV" => "SV",
        "TR" => "TR",
        "UK" => "UK",
        _ => "ZH"
    };
}