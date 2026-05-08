using System.Text;
using CDR_AI_Assistant.Services.Interfaces;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services.AIServices;

public class StableDiffusionService : IImageGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _config;

    public string ProviderName => "Stable Diffusion";

    public StableDiffusionService(HttpClient httpClient, ConfigManager config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GenerateTextAsync(string prompt, string model, CancellationToken ct = default)
    {
        return await GenerateImageAsync(prompt, string.Empty, "512x512", ct);
    }

    public async Task<string> GenerateImageAsync(
        string prompt,
        string negativePrompt,
        string size,
        CancellationToken ct = default)
    {
        var cfg = _config.Load();

        var sizeParts = size.Split('x');
        var width = int.Parse(sizeParts[0]);
        var height = int.Parse(sizeParts[1]);

        var requestBody = new
        {
            prompt = prompt,
            negative_prompt = negativePrompt,
            width = width,
            height = height,
            steps = 20,
            cfg_scale = 7.0
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{cfg.BaseURL}/sdapi/v1/txt2img",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        return result.images[0].ToString();
    }

    public async Task<string> GenerateImageToImageAsync(
        string inputImageBase64,
        string prompt,
        string negativePrompt,
        string size,
        CancellationToken ct = default)
    {
        var cfg = _config.Load();

        var sizeParts = size.Split('x');
        var width = int.Parse(sizeParts[0]);
        var height = int.Parse(sizeParts[1]);

        var requestBody = new
        {
            init_images = new[] { inputImageBase64 },
            prompt = prompt,
            negative_prompt = negativePrompt,
            width = width,
            height = height,
            steps = 20,
            cfg_scale = 7.0,
            denoising_strength = 0.75
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{cfg.BaseURL}/sdapi/v1/img2img",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<dynamic>(json)
            ?? throw new InvalidOperationException("解析响应失败");

        return result.images[0].ToString();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var cfg = _config.Load();
            var response = await _httpClient.GetAsync(
                $"{cfg.BaseURL}/sdapi/v1/options",
                new CancellationTokenSource(5000).Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}