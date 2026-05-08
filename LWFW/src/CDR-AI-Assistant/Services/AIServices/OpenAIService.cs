using System.Net.Http.Headers;
using System.Text;
using CDR_AI_Assistant.Services.Interfaces;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services.AIServices;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "OpenAI";

    public OpenAIService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GenerateTextAsync(string prompt, string model, CancellationToken ct = default)
    {
        var cfg = _config.Load();
        var apiKey = string.IsNullOrEmpty(cfg.EncryptedApiKey)
            ? cfg.ApiKey
            : _config.Decrypt(cfg.EncryptedApiKey);

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API Key 未配置，请在设置中配置您的 OpenAI API Key");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 1000
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{cfg.BaseURL}/chat/completions",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        return result.choices[0].message.content.ToString();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var cfg = _config.Load();
            var apiKey = string.IsNullOrEmpty(cfg.EncryptedApiKey)
                ? cfg.ApiKey
                : _config.Decrypt(cfg.EncryptedApiKey);

            if (string.IsNullOrEmpty(apiKey))
                return false;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new { model = "gpt-4o", messages = new[] { new { role = "user", content = "Hi" } }, max_tokens = 5 };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"{cfg.BaseURL}/chat/completions",
                content,
                new CancellationTokenSource(5000).Token);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}