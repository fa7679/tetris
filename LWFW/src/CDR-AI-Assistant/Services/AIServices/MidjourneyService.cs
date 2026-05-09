using System.Net.Http.Headers;
using System.Text;
using CDR_AI_Assistant.Services.Interfaces;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services.AIServices;

public class MidjourneyService : IImageGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "Midjourney";

    public MidjourneyService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GenerateTextAsync(string prompt, string model, CancellationToken ct = default)
    {
        return await GenerateImageAsync(prompt, string.Empty, "1024x1024", ct);
    }

    public async Task<string> GenerateTextWithContextAsync(string prompt, string context, string model, CancellationToken ct = default)
    {
        return await GenerateImageAsync(prompt, string.Empty, "1024x1024", ct);
    }

    public async Task<string> GenerateImageAsync(
        string prompt,
        string negativePrompt,
        string size,
        CancellationToken ct = default)
    {
        var cfg = _config.Load();

        if (string.IsNullOrEmpty(cfg.MidjourneyApiUrl))
            throw new InvalidOperationException("Midjourney API URL 未配置，请在设置中配置");

        var (width, height) = ParseSize(size);

        var requestBody = new
        {
            prompt = prompt,
            negative_prompt = negativePrompt,
            width = width,
            height = height,
            callback_url = cfg.MidjourneyCallbackUrl
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{cfg.MidjourneyApiUrl}/imagine",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        var taskId = result.task_id?.ToString();

        for (int i = 0; i < 60; i++)
        {
            await Task.Delay(2000, ct);

            var statusResponse = await _httpClient.GetAsync(
                $"{cfg.MidjourneyApiUrl}/tasks/{taskId}",
                ct);

            if (statusResponse.IsSuccessStatusCode)
            {
                var statusJson = await statusResponse.Content.ReadAsStringAsync(ct);
                var statusResult = JsonConvert.DeserializeObject<dynamic>(statusJson);

                if (statusResult?.status == "completed")
                {
                    return statusResult.image_url?.ToString() ?? string.Empty;
                }
            }
        }

        return taskId ?? string.Empty;
    }

    public async Task<string> GenerateImageToImageAsync(
        string inputImageBase64,
        string prompt,
        string negativePrompt,
        string size,
        CancellationToken ct = default)
    {
        var cfg = _config.Load();

        if (string.IsNullOrEmpty(cfg.MidjourneyApiUrl))
            throw new InvalidOperationException("Midjourney API URL 未配置，请在设置中配置");

        var (width, height) = ParseSize(size);

        var requestBody = new
        {
            init_image = inputImageBase64,
            prompt = prompt,
            negative_prompt = negativePrompt,
            width = width,
            height = height,
            callback_url = cfg.MidjourneyCallbackUrl
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{cfg.MidjourneyApiUrl}/describe",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        var taskId = result.task_id?.ToString();

        for (int i = 0; i < 60; i++)
        {
            await Task.Delay(2000, ct);

            var statusResponse = await _httpClient.GetAsync(
                $"{cfg.MidjourneyApiUrl}/tasks/{taskId}",
                ct);

            if (statusResponse.IsSuccessStatusCode)
            {
                var statusJson = await statusResponse.Content.ReadAsStringAsync(ct);
                var statusResult = JsonConvert.DeserializeObject<dynamic>(statusJson);

                if (statusResult?.status == "completed")
                {
                    return statusResult.image_url?.ToString() ?? string.Empty;
                }
            }
        }

        return taskId ?? string.Empty;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var cfg = _config.Load();
            if (string.IsNullOrEmpty(cfg.MidjourneyApiUrl))
                return false;

            var response = await _httpClient.GetAsync(
                $"{cfg.MidjourneyApiUrl}/status",
                new CancellationTokenSource(5000).Token);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private (int width, int height) ParseSize(string size)
    {
        var parts = size.Split('x');
        var width = parts.Length > 0 ? int.Parse(parts[0]) : 1024;
        var height = parts.Length > 1 ? int.Parse(parts[1]) : 1024;
        return (width, height);
    }
}