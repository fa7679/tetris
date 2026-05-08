namespace CDR_AI_Assistant.Services.Interfaces;

public interface IImageGeneratorService : IAIService
{
    Task<string> GenerateImageAsync(string prompt, string negativePrompt, string size, CancellationToken ct = default);
    Task<string> GenerateImageToImageAsync(string inputImageBase64, string prompt, string negativePrompt, string size, CancellationToken ct = default);
}