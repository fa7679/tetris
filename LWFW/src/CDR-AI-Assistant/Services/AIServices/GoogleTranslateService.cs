using System.Net.Http.Headers;
using System.Text;
using CDR_AI_Assistant.Services.Interfaces;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services.AIServices;

public class GoogleTranslateService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "Google Translate";

    public GoogleTranslateService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> TranslateAsync(string text, string targetLang = "ZH", CancellationToken ct = default)
    {
        var cfg = _config.Load();

        if (string.IsNullOrEmpty(cfg.GoogleTranslateApiKey))
            throw new InvalidOperationException("Google Translate API Key 未配置，请在设置中配置");

        var langCode = MapToGoogleLangCode(targetLang);

        var requestBody = new
        {
            q = text,
            target = langCode,
            format = "text"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var url = $"https://translation.googleapis.com/language/translate/v2?key={cfg.GoogleTranslateApiKey}";
        var response = await _httpClient.PostAsync(url, content, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("翻译解析失败");

        return result.data.translations[0].translatedText.ToString();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var cfg = _config.Load();
            if (string.IsNullOrEmpty(cfg.GoogleTranslateApiKey))
                return false;

            var requestBody = new { q = "Hello", target = "de", format = "text" };
            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"https://translation.googleapis.com/language/translate/v2?key={cfg.GoogleTranslateApiKey}",
                content,
                new CancellationTokenSource(5000).Token);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string MapToGoogleLangCode(string code) => code switch
    {
        "ZH" => "zh",
        "EN" => "en",
        "JA" => "ja",
        "KO" => "ko",
        "FR" => "fr",
        "DE" => "de",
        "ES" => "es",
        "IT" => "it",
        "NL" => "nl",
        "PL" => "pl",
        "PT" => "pt",
        "RU" => "ru",
        "SV" => "sv",
        "TR" => "tr",
        "UK" => "uk",
        _ => "zh"
    };
}