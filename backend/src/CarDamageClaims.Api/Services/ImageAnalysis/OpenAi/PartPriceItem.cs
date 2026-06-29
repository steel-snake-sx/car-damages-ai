using System.Text.Json.Serialization;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public sealed class PartPriceItem
{
    [JsonPropertyName("part_name")]
    public string PartName { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
