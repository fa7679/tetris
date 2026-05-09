namespace CDR_AI_Assistant.Models;

public class AppConfig
{
    // AI 服务配置
    public string BaseURL { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o";
    public string SelectedAIProvider { get; set; } = "OpenAI";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
    public string DefaultImageSize { get; set; } = "512x512";

    // DeepL 翻译配置
    public string DeepLApiKey { get; set; } = string.Empty;
    public string DeepLApiKeyEncrypted { get; set; } = string.Empty;

    // Google 翻译配置
    public string GoogleTranslateApiKey { get; set; } = string.Empty;

    // Midjourney 配置
    public bool MidjourneyEnabled { get; set; } = false;
    public string MidjourneyApiUrl { get; set; } = string.Empty;
    public string MidjourneyCallbackUrl { get; set; } = string.Empty;

    // 导出配置
    public string DefaultExportFormat { get; set; } = "PNG";
    public string LastExportFolder { get; set; } = string.Empty;

    // 翻译服务选择
    public string SelectedTranslationProvider { get; set; } = "DeepL";
}