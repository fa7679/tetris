namespace CDR_AI_Assistant.Services.Interfaces;

public interface ITranslationService
{
    string ProviderName { get; }
    Task<string> TranslateAsync(string text, string targetLang, CancellationToken ct = default);
    Task<bool> TestConnectionAsync();
}