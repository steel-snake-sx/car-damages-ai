namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class OpenAiResponseContentReader
{
    public string ExtractJson(string? outputText, IReadOnlyList<string?> outputContentTexts)
    {
        string rawText;

        if (!string.IsNullOrWhiteSpace(outputText))
        {
            rawText = outputText;
        }
        else
        {
            var text = outputContentTexts.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("OpenAI response text is empty.");
            }

            rawText = text;
        }

        var normalized = rawText.Trim();

        if (normalized.StartsWith("```", StringComparison.Ordinal))
        {
            normalized = normalized
                .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("```", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        var firstBrace = normalized.IndexOf('{');
        var lastBrace = normalized.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            normalized = normalized.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return normalized;
    }
}
