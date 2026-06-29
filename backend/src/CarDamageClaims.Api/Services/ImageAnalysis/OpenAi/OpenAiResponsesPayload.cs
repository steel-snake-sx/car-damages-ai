namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class OpenAiResponsesPayload
{
    public object CreateResponsesRequest(
        IReadOnlyList<string> filePaths,
        string instruction,
        string model,
        int maxOutputTokens,
        bool includeWebSearch,
        bool forceJsonFormat
    )
    {
        var inputContent = new List<object> { new { type = "input_text", text = instruction } };

        foreach (var filePath in filePaths.Take(3))
        {
            var fileBytes = File.ReadAllBytes(filePath);
            var mimeType = ResolveMimeType(filePath);
            var dataUri = $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes)}";
            inputContent.Add(new { type = "input_image", image_url = dataUri });
        }

        var tools = includeWebSearch
            ? new object[] { new { type = "web_search" } }
            : Array.Empty<object>();

        return new
        {
            model,
            max_output_tokens = maxOutputTokens,
            tools,
            text = forceJsonFormat ? new { format = new { type = "json_object" } } : null,
            input = new[] { new { role = "user", content = inputContent } },
        };
    }

    public string ResolveMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "image/jpeg",
        };
    }
}
