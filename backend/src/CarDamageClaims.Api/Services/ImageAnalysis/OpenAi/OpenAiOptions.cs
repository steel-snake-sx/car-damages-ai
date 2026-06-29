namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string? ProxyUrl { get; set; }

    public string Model { get; set; } = "gpt-4o";

    public int AnalysisMaxOutputTokens { get; set; } = 420;

    public int PricingMaxOutputTokens { get; set; } = 700;
}
