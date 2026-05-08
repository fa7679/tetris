namespace CDR_AI_Assistant.Models;

public class AIRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string NegativePrompt { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1000;
}

public class AIImageRequest : AIRequest
{
    public int Steps { get; set; } = 20;
    public double CfgScale { get; set; } = 7.0;
    public string? InputImageBase64 { get; set; }
}