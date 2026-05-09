using System.Net.Http.Headers;
using System.Text;
using CDR_AI_Assistant.Services.Interfaces;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services.AIServices;

public class ClaudeService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "Claude";

    public ClaudeService(HttpClient httpClient, ConfigManager config)
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
            throw new InvalidOperationException("API Key 未配置，请在设置中配置您的 Claude API Key");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new
        {
            model = model ?? "claude-3-sonnet-20240229",
            max_tokens = 1000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            "https://api.anthropic.com/v1/messages",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        return result.content[0].text.ToString();
    }

    public async Task<string> GenerateTextWithContextAsync(string prompt, string context, string model, CancellationToken ct = default)
    {
        var cfg = _config.Load();
        var apiKey = string.IsNullOrEmpty(cfg.EncryptedApiKey)
            ? cfg.ApiKey
            : _config.Decrypt(cfg.EncryptedApiKey);

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API Key 未配置，请在设置中配置您的 Claude API Key");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var messages = new List<object>();
        if (!string.IsNullOrEmpty(context))
        {
            messages.Add(new { role = "user", content = $"参考内容：\n{context}\n\n请根据以上内容生成文案。" });
        }
        messages.Add(new { role = "user", content = prompt });

        var requestBody = new
        {
            model = model ?? "claude-3-sonnet-20240229",
            max_tokens = 1000,
            messages = messages.ToArray()
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            "https://api.anthropic.com/v1/messages",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        return result.content[0].text.ToString();
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
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var requestBody = new
            {
                model = "claude-3-sonnet-20240229",
                max_tokens = 5,
                messages = new[] { new { role = "user", content = "Hi" } }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.anthropic.com/v1/messages",
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