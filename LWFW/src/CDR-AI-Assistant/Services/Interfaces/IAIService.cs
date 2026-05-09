namespace CDR_AI_Assistant.Services.Interfaces;

public interface IAIService
{
    string ProviderName { get; }
    Task<string> GenerateTextAsync(string prompt, string model, CancellationToken ct = default);
    Task<string> GenerateTextWithContextAsync(string prompt, string context, string model, CancellationToken ct = default);
    Task<bool> TestConnectionAsync();
}